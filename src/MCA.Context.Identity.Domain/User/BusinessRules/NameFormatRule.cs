/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Business Rule: Name Format Validation
 * Year: 2025
 */

using System.Text.RegularExpressions;
using MCA.Context.Identity.Domain.User.Constants;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.BusinessRules;

/// <summary>
/// Business rule for validating name format.
/// Ensures names follow proper format and length requirements.
/// </summary>
public class NameFormatRule : IBusinessRule
{
    private readonly string _name;
    private readonly string _fieldName;
    private readonly int _minLength;
    private readonly int _maxLength;
    private static readonly Regex NameRegex = new(ValidationConstants.NamePattern, RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the NameFormatRule.
    /// </summary>
    /// <param name="name">The name string to validate.</param>
    /// <param name="fieldName">The name of the field being validated (e.g., "First name", "Last name").</param>
    /// <param name="minLength">Minimum length requirement.</param>
    /// <param name="maxLength">Maximum length requirement.</param>
    public NameFormatRule(string name, string fieldName, int minLength, int maxLength)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        _minLength = minLength;
        _maxLength = maxLength;
    }

    /// <summary>
    /// Creates a rule for first name validation.
    /// </summary>
    /// <param name="firstName">The first name to validate.</param>
    /// <returns>A NameFormatRule configured for first name validation.</returns>
    public static NameFormatRule ForFirstName(string firstName)
    {
        return new NameFormatRule(
            firstName, 
            "First name", 
            ValidationConstants.FirstNameMinLength, 
            ValidationConstants.FirstNameMaxLength);
    }

    /// <summary>
    /// Creates a rule for last name validation.
    /// </summary>
    /// <param name="lastName">The last name to validate.</param>
    /// <returns>A NameFormatRule configured for last name validation.</returns>
    public static NameFormatRule ForLastName(string lastName)
    {
        return new NameFormatRule(
            lastName, 
            "Last name", 
            ValidationConstants.LastNameMinLength, 
            ValidationConstants.LastNameMaxLength);
    }

    /// <summary>
    /// Determines if the name format rule is satisfied.
    /// </summary>
    /// <returns>True if the name format is valid; otherwise, false.</returns>
    public bool IsSatisfied()
    {
        if (string.IsNullOrWhiteSpace(_name))
            return false;

        if (_name.Length < _minLength)
            return false;

        if (_name.Length > _maxLength)
            return false;

        return NameRegex.IsMatch(_name);
    }

    /// <summary>
    /// Gets the error message when the rule is not satisfied.
    /// </summary>
    public string Message
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_name))
                return ValidationConstants.NameEmptyErrorMessage;

            if (_name.Length < _minLength)
            {
                return _fieldName.ToLower() switch
                {
                    "first name" => string.Format(ValidationConstants.FirstNameTooShortErrorMessage, _minLength),
                    "last name" => string.Format(ValidationConstants.LastNameTooShortErrorMessage, _minLength),
                    _ => $"{_fieldName} must be at least {_minLength} characters long"
                };
            }

            if (_name.Length > _maxLength)
            {
                return _fieldName.ToLower() switch
                {
                    "first name" => string.Format(ValidationConstants.FirstNameTooLongErrorMessage, _maxLength),
                    "last name" => string.Format(ValidationConstants.LastNameTooLongErrorMessage, _maxLength),
                    _ => $"{_fieldName} cannot exceed {_maxLength} characters"
                };
            }

            return ValidationConstants.NameInvalidFormatErrorMessage;
        }
    }
} 