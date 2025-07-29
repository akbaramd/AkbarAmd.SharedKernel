/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Business Rule: Password Strength Validation
 * Year: 2025
 */

using MCA.Identity.Domain.User.Constants;
using MCA.SharedKernel.Domain.Contracts;
using System.Text.RegularExpressions;

namespace MCA.Identity.Domain.User.BusinessRules;

/// <summary>
/// Business rule for validating password strength.
/// Ensures password meets security requirements including length, complexity, and character types.
/// </summary>
public class PasswordStrengthRule : IBusinessRule
{
    private readonly string _password;
    private static readonly Regex PasswordRegex = new(ValidationConstants.PasswordPattern, RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the PasswordStrengthRule.
    /// </summary>
    /// <param name="password">The password string to validate.</param>
    public PasswordStrengthRule(string password)
    {
        _password = password ?? throw new ArgumentNullException(nameof(password));
    }

    /// <summary>
    /// Determines if the password strength rule is satisfied.
    /// </summary>
    /// <returns>True if the password meets strength requirements; otherwise, false.</returns>
    public bool IsSatisfied()
    {
        if (string.IsNullOrWhiteSpace(_password))
            return false;

        if (_password.Length < ValidationConstants.PasswordMinLength)
            return false;

        if (_password.Length > ValidationConstants.PasswordMaxLength)
            return false;

        // Check for required character types
        if (!HasRequiredUppercase())
            return false;

        if (!HasRequiredLowercase())
            return false;

        if (!HasRequiredDigits())
            return false;

        if (!HasRequiredSpecialCharacters())
            return false;

        return true;
    }

    /// <summary>
    /// Gets the error message when the rule is not satisfied.
    /// </summary>
    public string Message
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_password))
                return ValidationConstants.PasswordEmptyErrorMessage;

            if (_password.Length < ValidationConstants.PasswordMinLength)
                return string.Format(ValidationConstants.PasswordTooShortErrorMessage, ValidationConstants.PasswordMinLength);

            if (_password.Length > ValidationConstants.PasswordMaxLength)
                return string.Format(ValidationConstants.PasswordTooLongErrorMessage, ValidationConstants.PasswordMaxLength);

            if (!HasRequiredUppercase())
                return string.Format(ValidationConstants.PasswordMissingUppercaseErrorMessage, ValidationConstants.PasswordMinUppercase);

            if (!HasRequiredLowercase())
                return string.Format(ValidationConstants.PasswordMissingLowercaseErrorMessage, ValidationConstants.PasswordMinLowercase);

            if (!HasRequiredDigits())
                return string.Format(ValidationConstants.PasswordMissingDigitsErrorMessage, ValidationConstants.PasswordMinDigits);

            if (!HasRequiredSpecialCharacters())
                return string.Format(ValidationConstants.PasswordMissingSpecialCharsErrorMessage, ValidationConstants.PasswordMinSpecialChars);

            return ValidationConstants.PasswordWeakErrorMessage;
        }
    }

    /// <summary>
    /// Checks if the password contains the required number of uppercase letters.
    /// </summary>
    /// <returns>True if the password has enough uppercase letters; otherwise, false.</returns>
    private bool HasRequiredUppercase()
    {
        var uppercaseCount = _password.Count(char.IsUpper);
        return uppercaseCount >= ValidationConstants.PasswordMinUppercase;
    }

    /// <summary>
    /// Checks if the password contains the required number of lowercase letters.
    /// </summary>
    /// <returns>True if the password has enough lowercase letters; otherwise, false.</returns>
    private bool HasRequiredLowercase()
    {
        var lowercaseCount = _password.Count(char.IsLower);
        return lowercaseCount >= ValidationConstants.PasswordMinLowercase;
    }

    /// <summary>
    /// Checks if the password contains the required number of digits.
    /// </summary>
    /// <returns>True if the password has enough digits; otherwise, false.</returns>
    private bool HasRequiredDigits()
    {
        var digitCount = _password.Count(char.IsDigit);
        return digitCount >= ValidationConstants.PasswordMinDigits;
    }

    /// <summary>
    /// Checks if the password contains the required number of special characters.
    /// </summary>
    /// <returns>True if the password has enough special characters; otherwise, false.</returns>
    private bool HasRequiredSpecialCharacters()
    {
        var specialCharCount = _password.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        return specialCharCount >= ValidationConstants.PasswordMinSpecialChars;
    }
} 