using System.Text.RegularExpressions;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies;

public class PasswordPolicy
{
    private const int MinLength = 8;
    private const int MaxLength = 128;
    
    public static PasswordValidationResult Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordValidationResult.Failure("Password cannot be empty.");

        if (password.Length < MinLength)
            return PasswordValidationResult.Failure($"Password must be at least {MinLength} characters long.");

        if (password.Length > MaxLength)
            return PasswordValidationResult.Failure($"Password cannot exceed {MaxLength} characters.");

        if (!HasUpperCase(password))
            return PasswordValidationResult.Failure("Password must contain at least one uppercase letter.");

        if (!HasLowerCase(password))
            return PasswordValidationResult.Failure("Password must contain at least one lowercase letter.");

        if (!HasNumber(password))
            return PasswordValidationResult.Failure("Password must contain at least one number.");

        if (!HasSpecialCharacter(password))
            return PasswordValidationResult.Failure("Password must contain at least one special character.");

        return PasswordValidationResult.Success();
    }

    private static bool HasUpperCase(string password) => password.Any(char.IsUpper);
    private static bool HasLowerCase(string password) => password.Any(char.IsLower);
    private static bool HasNumber(string password) => password.Any(char.IsDigit);
    private static bool HasSpecialCharacter(string password) 
        => Regex.IsMatch(password, @"[!@#$%^&*(),.?"":{}|<>]");
}

public class PasswordValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private PasswordValidationResult(bool isValid, string errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static PasswordValidationResult Success() 
        => new(true);

    public static PasswordValidationResult Failure(string errorMessage) 
        => new(false, errorMessage);
} 