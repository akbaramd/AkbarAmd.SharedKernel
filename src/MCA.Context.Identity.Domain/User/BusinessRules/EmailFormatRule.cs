/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Business Rule: Email Format Validation
 * Year: 2025
 */

using MCA.Identity.Domain.User.Constants;
using MCA.Identity.Domain.User.ValueObjects;
using MCA.SharedKernel.Domain.Contracts;
using System.Text.RegularExpressions;

namespace MCA.Identity.Domain.User.BusinessRules;

/// <summary>
/// Business rule for validating email format.
/// Ensures email follows proper format and length requirements.
/// </summary>
public class EmailFormatRule : IBusinessRule
{
    private readonly string _email;
    private static readonly Regex EmailRegex = new(ValidationConstants.EmailPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Initializes a new instance of the EmailFormatRule.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    public EmailFormatRule(string email)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
    }

    /// <summary>
    /// Determines if the email format rule is satisfied.
    /// </summary>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    public bool IsSatisfied()
    {
        if (string.IsNullOrWhiteSpace(_email))
            return false;

        if (_email.Length < ValidationConstants.EmailMinLength)
            return false;

        if (_email.Length > ValidationConstants.EmailMaxLength)
            return false;

        return EmailRegex.IsMatch(_email);
    }

    /// <summary>
    /// Gets the error message when the rule is not satisfied.
    /// </summary>
    public string Message
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_email))
                return ValidationConstants.EmailEmptyErrorMessage;

            if (_email.Length < ValidationConstants.EmailMinLength)
                return string.Format(ValidationConstants.EmailTooShortErrorMessage, ValidationConstants.EmailMinLength);

            if (_email.Length > ValidationConstants.EmailMaxLength)
                return string.Format(ValidationConstants.EmailTooLongErrorMessage, ValidationConstants.EmailMaxLength);

            return ValidationConstants.EmailFormatErrorMessage;
        }
    }
} 