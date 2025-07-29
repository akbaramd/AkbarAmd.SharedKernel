/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Value Object: Email
 * Year: 2025
 */

using MCA.Identity.Domain.User.BusinessRules;
using MCA.Identity.Domain.User.Constants;
using MCA.SharedKernel.Domain;
using System.Text.RegularExpressions;

namespace MCA.Identity.Domain.User.ValueObjects;

/// <summary>
/// Value object representing an email address.
/// Implements proper validation using business rules and constants.
/// </summary>
public sealed class Email : ValueObject
{
    /// <summary>
    /// The email address value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for creating Email instances.
    /// </summary>
    /// <param name="value">The email address value.</param>
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Email instance with validation.
    /// </summary>
    /// <param name="email">The email address string.</param>
    /// <returns>A new Email instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    /// <exception cref="ArgumentException">Thrown when email format is invalid.</exception>
    public static Email Create(string email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        var trimmed = email.Trim();
        var instance = new Email(trimmed.ToLowerInvariant());
        
        // Validate using business rule
        Email.CheckRule(new EmailFormatRule(instance.Value));
        
        return instance;
    }

    /// <summary>
    /// Validates the email format using business rules.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when email format is invalid.</exception>
    public override void Validate()
    {
        ValueObject.CheckRule(new EmailFormatRule(Value));
    }

    /// <summary>
    /// Gets the equality components for value object comparison.
    /// </summary>
    /// <returns>Enumerable of equality components.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns a string representation of the email.
    /// </summary>
    /// <returns>The email address as a string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicit conversion from Email to string.
    /// </summary>
    /// <param name="email">The Email instance.</param>
    public static implicit operator string(Email email) => email?.Value;

    /// <summary>
    /// Determines if the email is in a valid format.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    public static bool IsValidFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var rule = new EmailFormatRule(email);
        return rule.IsSatisfied();
    }

    /// <summary>
    /// Gets the validation error message for an invalid email.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns>The validation error message if invalid; otherwise, null.</returns>
    public static string? GetValidationErrorMessage(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return ValidationConstants.EmailEmptyErrorMessage;

        var rule = new EmailFormatRule(email);
        return rule.IsSatisfied() ? null : rule.Message;
    }
}
