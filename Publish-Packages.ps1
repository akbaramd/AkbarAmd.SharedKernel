# Publish-Packages.ps1
# Developed by Akbar Ahmadi Saray
# Script to pack and publish CleanArchitecture projects as NuGet packages
# Usage: Run from solution root

$projects = @(
    "src\SharedKernel\AkbarAmd.SharedKernel.Domain",
    "src\SharedKernel\AkbarAmd.SharedKernel.Application",
    "src\SharedKernel\AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore"
)

$nupkgOutput = Join-Path -Path (Get-Location) -ChildPath "nupkgs"

# Ensure nupkgs folder exists
if (-Not (Test-Path $nupkgOutput)) {
    New-Item -ItemType Directory -Path $nupkgOutput | Out-Null
}

foreach ($proj in $projects) {
    Write-Host "Packing $proj..."
    Push-Location $proj

    dotnet pack -c Release -o $nupkgOutput

    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet pack failed for $proj"
        Pop-Location
        exit $LASTEXITCODE
    }

    Pop-Location
}

Write-Host "All packages packed successfully and output to $nupkgOutput"
