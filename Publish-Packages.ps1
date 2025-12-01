# Publish-Packages.ps1
# Developed by Akbar Ahmadi Saray
# Script to pack and publish CleanArchitecture projects as NuGet packages
# Usage: Run from solution root

param(
    [switch]$PublishToNuGet,
    [switch]$SkipConfirmation,
    # Default: read from environment variable NUGETKEY (GitLab) or NUGET_API_KEY (GitHub Actions)
    [string]$NuGetApiKey = $env:NUGETKEY ?? $env:NUGET_API_KEY,
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = "Continue"

# Function to pack a .NET project
function Pack-DotnetProject {
    param(
        [string]$projectPath,
        [string]$projectName,
        [string]$outputPath,
        [string]$basePath
    )
    
    $originalLocation = Get-Location
    $absoluteProjectPath = Join-Path -Path $basePath -ChildPath $projectPath
    
    if (-not (Test-Path $absoluteProjectPath)) {
        throw "Project path does not exist: $absoluteProjectPath"
    }
    
    try {
        Set-Location -Path $absoluteProjectPath | Out-Null
        
        Write-Host "      Cleaning previous builds..." -ForegroundColor Gray
        dotnet clean -c Release | Out-Null
        
        Write-Host "      Restoring packages..." -ForegroundColor Gray
        dotnet restore | Out-Null
        if ($LASTEXITCODE -ne 0) { 
            throw "dotnet restore failed" 
        }
        
        Write-Host "      Building..." -ForegroundColor Gray
        $buildOutput = dotnet build -c Release --no-restore 2>&1 | Out-String
        if ($LASTEXITCODE -ne 0) { 
            Write-Host $buildOutput -ForegroundColor Red
            throw "dotnet build failed" 
        }
        
        Write-Host "      Packing..." -ForegroundColor Gray
        $packResult = dotnet pack -c Release --no-build -o $outputPath 2>&1 | Out-String
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "dotnet pack failed for $projectName"
            Write-Host $packResult -ForegroundColor Red
            throw "Pack failed"
        }
        
        $nupkgFile = Get-ChildItem -Path $outputPath -Filter "$projectName.*.nupkg" -ErrorAction SilentlyContinue | 
                     Sort-Object LastWriteTime -Descending | 
                     Select-Object -First 1
        
        if ($nupkgFile) {
            Write-Host "      Packed: $($nupkgFile.Name)" -ForegroundColor Green
            return $nupkgFile.FullName
        } else {
            Write-Warning "      Could not find generated .nupkg file for $projectName"
            return $null
        }
    }
    catch {
        Write-Error "Failed to pack $projectName : $_"
        return $null
    }
    finally {
        $currentPath = (Get-Location).Path
        $originalPath = $originalLocation.Path
        if ($currentPath -ne $originalPath) {
            Set-Location -Path $originalLocation -ErrorAction SilentlyContinue | Out-Null
        }
    }
}

# Function to run all tests and ensure they pass
function Run-Tests {
    param(
        [string]$basePath
    )
    
    Write-Host ""
    Write-Host "=== Running All Tests ===" -ForegroundColor Yellow
    Write-Host "      This ensures all tests pass before packaging..." -ForegroundColor Gray
    Write-Host ""
    
    try {
        $testOutput = dotnet test --verbosity normal 2>&1 | Out-String
        
        # Check if tests passed by looking for test summary
        if ($LASTEXITCODE -ne 0) {
            Write-Host $testOutput -ForegroundColor Red
            Write-Error "Tests failed! Cannot proceed with packaging. Please fix all failing tests first."
            return $false
        }
        
        # Parse test results from output
        if ($testOutput -match "Test summary:.*total:\s*(\d+).*failed:\s*(\d+).*succeeded:\s*(\d+)") {
            $totalTests = $matches[1]
            $failedTests = $matches[2]
            $succeededTests = $matches[3]
            
            Write-Host "      Test Results:" -ForegroundColor Gray
            Write-Host "        Total: $totalTests" -ForegroundColor Gray
            Write-Host "        Succeeded: $succeededTests" -ForegroundColor Green
            Write-Host "        Failed: $failedTests" -ForegroundColor $(if ($failedTests -eq "0") { "Green" } else { "Red" })
            Write-Host ""
            
            if ($failedTests -ne "0") {
                Write-Error "Tests failed! Cannot proceed with packaging. Please fix all failing tests first."
                return $false
            }
            
            Write-Host "      All tests passed! Proceeding with packaging..." -ForegroundColor Green
            Write-Host ""
            return $true
        } else {
            # If we can't parse the output but exit code is 0, assume success
            Write-Host "      Tests completed successfully!" -ForegroundColor Green
            Write-Host ""
            return $true
        }
    }
    catch {
        Write-Error "Failed to run tests: $_"
        return $false
    }
}

# Function to publish a package to NuGet
function Publish-PackageToNuGet {
    param(
        [string]$packagePath,
        [string]$packageName,
        [string]$apiKey,
        [string]$source
    )
    
    try {
        $nugetExe = Get-Command nuget -ErrorAction SilentlyContinue
        $useNugetExe = $null -ne $nugetExe
        
        if ($useNugetExe) {
            $pushCommand = "nuget push `"$packagePath`" -Source $source -ApiKey $apiKey -SkipDuplicate"
        } else {
            $pushCommand = "dotnet nuget push `"$packagePath`" --source $source --api-key $apiKey --skip-duplicate"
        }
        
        $pushOutput = Invoke-Expression -Command $pushCommand 2>&1 | Out-String
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "      Published successfully!" -ForegroundColor Green
            return $true
        } elseif ($LASTEXITCODE -eq 1) {
            if ($pushOutput -match "already exists" -or $pushOutput -match "already been pushed" -or $pushOutput -match "already been uploaded") {
                Write-Host "      Package version already exists (skipped)" -ForegroundColor Yellow
                return $true
            } else {
                Write-Warning "      Publishing returned exit code 1"
                return $false
            }
        } else {
            Write-Error "      Publishing failed for $packageName"
            Write-Host $pushOutput -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Error "      Publishing failed for $packageName : $_"
        return $false
    }
}

# Main script execution
$scriptRoot = Get-Location

# Run tests first - ALL TESTS MUST PASS before packaging
$testsPassed = Run-Tests -basePath $scriptRoot
if (-not $testsPassed) {
    Write-Host ""
    Write-Error "Cannot proceed with packaging. All tests must pass first."
    exit 1
}

$projects = @(
    @{ Path = "src\SharedKernel\AkbarAmd.SharedKernel.Domain"; Name = "AkbarAmd.SharedKernel.Domain"; Order = 1 },
    @{ Path = "src\SharedKernel\AkbarAmd.SharedKernel.Application"; Name = "AkbarAmd.SharedKernel.Application"; Order = 2 },
    @{ Path = "src\SharedKernel\AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore"; Name = "AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore"; Order = 3 }
)

$projects = $projects | Sort-Object -Property Order
$nupkgOutput = Join-Path -Path $scriptRoot -ChildPath "nupkgs"

if (-Not (Test-Path $nupkgOutput)) {
    New-Item -ItemType Directory -Path $nupkgOutput -Force | Out-Null
    Write-Host "Created output directory: $nupkgOutput" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Starting Package Build and Pack Process ===" -ForegroundColor Yellow
Write-Host "Output directory: $nupkgOutput" -ForegroundColor Gray
Write-Host ""

$packedPackages = @()

foreach ($proj in $projects) {
    $projPath = $proj.Path
    $projName = $proj.Name
    
    Write-Host ""
    Write-Host "[2/3] Packing $projName..." -ForegroundColor Cyan
    Write-Host "      Project: $projPath" -ForegroundColor Gray
    
    $nupkgFile = Pack-DotnetProject -projectPath $projPath -projectName $projName -outputPath $nupkgOutput -basePath $scriptRoot
    
    if ($nupkgFile) {
        $packedPackages += @{ 
            Name = $projName
            File = $nupkgFile
            Path = $projPath
        }
    }
}

Write-Host ""
Write-Host "=== Package Packing Complete ===" -ForegroundColor Green
Write-Host "Packed $($packedPackages.Count) package(s) to: $nupkgOutput" -ForegroundColor Gray
Write-Host ""

if ($PublishToNuGet) {
    if ([string]::IsNullOrWhiteSpace($NuGetApiKey)) {
        Write-Error "NuGet API key is required when publishing. Set NUGETKEY (GitLab) or NUGET_API_KEY (GitHub) env var or pass -NuGetApiKey."
        exit 1
    }

    Write-Host ""
    Write-Host "=== Starting Package Publishing Process ===" -ForegroundColor Yellow
    Write-Host "Source: $Source" -ForegroundColor Gray
    Write-Host ""

    $nugetExe = Get-Command nuget -ErrorAction SilentlyContinue
    $useNugetExe = $null -ne $nugetExe

    if ($useNugetExe) {
        Write-Host "Using nuget.exe for publishing..." -ForegroundColor Gray
    } else {
        Write-Host "Using dotnet nuget for publishing (nuget.exe not found)..." -ForegroundColor Gray
    }

    if (-not $SkipConfirmation) {
        $confirmPublish = Read-Host "Are you sure you want to publish $($packedPackages.Count) package(s) to NuGet? (yes/no)"
        
        if ($confirmPublish -ne "yes") {
            Write-Host "Publishing cancelled." -ForegroundColor Yellow
            exit 0
        }
    } else {
        Write-Host "Skipping confirmation (auto-publishing)..." -ForegroundColor Gray
    }

    foreach ($package in $packedPackages) {
        Write-Host ""
        Write-Host "[3/3] Publishing $($package.Name)..." -ForegroundColor Cyan
        Write-Host "      File: $($package.File)" -ForegroundColor Gray

        $published = Publish-PackageToNuGet -packagePath $package.File -packageName $package.Name -apiKey $NuGetApiKey -source $Source
        
        if (-not $published) {
            Write-Host "Continuing with next package..." -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "=== Package Publishing Complete ===" -ForegroundColor Green
}

if (-not $PublishToNuGet) {
    Write-Host ""
    Write-Host "To publish packages to NuGet, run:" -ForegroundColor Yellow
    Write-Host "  .\Publish-Packages.ps1 -PublishToNuGet" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Note: In CI, configure the NuGet API key via NUGETKEY (GitLab) or NUGET_API_KEY (GitHub Actions) environment variable." -ForegroundColor Gray
    Write-Host ""
}
