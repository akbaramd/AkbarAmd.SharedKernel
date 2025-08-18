/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Policy: Password Validation Policy
 * Year: 2025
 */

using MCA.Context.Identity.Domain.User.Constants;

namespace MCA.Context.Identity.Domain.User.Policies;

/// <summary>
/// Policy for password validation that uses the same constants and rules as business rules.
/// This policy provides a higher-level interface for password validation.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>
    /// Validates a password using the same rules as PasswordStrengthRule.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <returns>A PasswordValidationResult indicating validation status.</returns>
    public static PasswordValidationResult Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordValidationResult.Failure(ValidationConstants.PasswordEmptyErrorMessage);

        if (password.Length < ValidationConstants.PasswordMinLength)
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordTooShortErrorMessage, ValidationConstants.PasswordMinLength));

        if (password.Length > ValidationConstants.PasswordMaxLength)
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordTooLongErrorMessage, ValidationConstants.PasswordMaxLength));

        if (!HasRequiredUppercase(password))
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordMissingUppercaseErrorMessage, ValidationConstants.PasswordMinUppercase));

        if (!HasRequiredLowercase(password))
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordMissingLowercaseErrorMessage, ValidationConstants.PasswordMinLowercase));

        if (!HasRequiredDigits(password))
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordMissingDigitsErrorMessage, ValidationConstants.PasswordMinDigits));

        if (!HasRequiredSpecialCharacters(password))
            return PasswordValidationResult.Failure(
                string.Format(ValidationConstants.PasswordMissingSpecialCharsErrorMessage, ValidationConstants.PasswordMinSpecialChars));

        return PasswordValidationResult.Success();
    }

    /// <summary>
    /// Checks if the password contains the required number of uppercase letters.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password has enough uppercase letters; otherwise, false.</returns>
    private static bool HasRequiredUppercase(string password)
    {
        var uppercaseCount = password.Count(char.IsUpper);
        return uppercaseCount >= ValidationConstants.PasswordMinUppercase;
    }

    /// <summary>
    /// Checks if the password contains the required number of lowercase letters.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password has enough lowercase letters; otherwise, false.</returns>
    private static bool HasRequiredLowercase(string password)
    {
        var lowercaseCount = password.Count(char.IsLower);
        return lowercaseCount >= ValidationConstants.PasswordMinLowercase;
    }

    /// <summary>
    /// Checks if the password contains the required number of digits.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password has enough digits; otherwise, false.</returns>
    private static bool HasRequiredDigits(string password)
    {
        var digitCount = password.Count(char.IsDigit);
        return digitCount >= ValidationConstants.PasswordMinDigits;
    }

    /// <summary>
    /// Checks if the password contains the required number of special characters.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password has enough special characters; otherwise, false.</returns>
    private static bool HasRequiredSpecialCharacters(string password)
    {
        var specialCharCount = password.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        return specialCharCount >= ValidationConstants.PasswordMinSpecialChars;
    }

    /// <summary>
    /// Determines if a password meets the strength requirements.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>True if the password meets strength requirements; otherwise, false.</returns>
    public static bool IsStrong(string password)
    {
        var result = Validate(password);
        return result.IsValid;
    }

    /// <summary>
    /// Gets the strength score of a password (0-100).
    /// </summary>
    /// <param name="password">The password to score.</param>
    /// <returns>A score from 0 to 100 indicating password strength.</returns>
    public static int GetStrengthScore(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return 0;

        var score = 0;

        // Length score (up to 25 points)
        if (password.Length >= ValidationConstants.PasswordMinLength)
            score += Math.Min(25, password.Length - ValidationConstants.PasswordMinLength + 1);

        // Character variety score (up to 50 points)
        if (HasRequiredUppercase(password)) score += 10;
        if (HasRequiredLowercase(password)) score += 10;
        if (HasRequiredDigits(password)) score += 10;
        if (HasRequiredSpecialCharacters(password)) score += 10;

        // Complexity bonus (up to 25 points)
        var uniqueChars = password.Distinct().Count();
        score += Math.Min(25, uniqueChars * 2);

        return Math.Min(100, score);
    }
}

/// <summary>
/// Result of password validation.
/// </summary>
public class PasswordValidationResult
{
    /// <summary>
    /// Gets whether the password is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of PasswordValidationResult.
    /// </summary>
    /// <param name="isValid">Whether the password is valid.</param>
    /// <param name="errorMessage">The error message if validation failed.</param>
    private PasswordValidationResult(bool isValid, string errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful PasswordValidationResult.</returns>
    public static PasswordValidationResult Success() 
        => new(true);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <returns>A failed PasswordValidationResult.</returns>
    public static PasswordValidationResult Failure(string errorMessage) 
        => new(false, errorMessage);
} 