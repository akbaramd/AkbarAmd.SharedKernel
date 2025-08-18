/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Value Object: Password
 * Year: 2025
 */

using System.Security.Cryptography;
using MCA.Context.Identity.Domain.User.BusinessRules;
using MCA.Context.Identity.Domain.User.Constants;
using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.ValueObjects;

/// <summary>
/// Value object representing a password.
/// Implements proper validation using business rules and secure hashing.
/// </summary>
public sealed class Password : ValueObject
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100_000; // Work factor

    /// <summary>
    /// The hashed password value in format: base64(salt):base64(hash).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Private constructor for creating Password instances.
    /// </summary>
    /// <param name="value">The hashed password value.</param>
    private Password(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new Password instance from a plain text password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>A new Password instance with hashed value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    /// <exception cref="ArgumentException">Thrown when password format is invalid.</exception>
    public static Password Create(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var trimmed = password.Trim();
        
        // Validate password strength before hashing
        ValueObject.CheckRule(new PasswordStrengthRule(trimmed));
        
        var hashedPassword = HashPassword(trimmed);
        return new Password(hashedPassword);
    }

    /// <summary>
    /// Creates a new Password instance from an existing hash.
    /// Use this method when restoring from database or external source.
    /// </summary>
    /// <param name="hashedPassword">The hashed password value.</param>
    /// <returns>A new Password instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when hashedPassword is null.</exception>
    /// <exception cref="ArgumentException">Thrown when hash format is invalid.</exception>
    public static Password FromHash(string hashedPassword)
    {
        if (hashedPassword is null)
            throw new ArgumentNullException(nameof(hashedPassword));

        var instance = new Password(hashedPassword);
        instance.Validate(); // Validate hash format
        return instance;
    }

    /// <summary>
    /// Hashes a plain text password using PBKDF2.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password string.</returns>
    private static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies if the provided plain text password matches this hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <returns>True if the password matches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    public bool Verify(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var parts = Value.Split(':');
        if (parts.Length != 2)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validates the password hash format using business rules.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when hash format is invalid.</exception>
    public override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentException("Password hash cannot be empty or whitespace", nameof(Value));

        var parts = Value.Split(':');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid password hash format: missing salt or hash.", nameof(Value));

        try
        {
            var saltBytes = Convert.FromBase64String(parts[0]);
            var hashBytes = Convert.FromBase64String(parts[1]);

            if (saltBytes.Length != SaltSize)
                throw new ArgumentException($"Invalid salt size. Expected {SaltSize} bytes.", nameof(Value));

            if (hashBytes.Length != HashSize)
                throw new ArgumentException($"Invalid hash size. Expected {HashSize} bytes.", nameof(Value));
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Password hash contains invalid Base64 strings.", nameof(Value), ex);
        }
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
    /// Returns a string representation of the password (masked for security).
    /// </summary>
    /// <returns>A masked string representation.</returns>
    public override string ToString() => "****"; // Never reveal hash string accidentally

    /// <summary>
    /// Determines if a plain text password meets strength requirements.
    /// </summary>
    /// <param name="password">The plain text password to validate.</param>
    /// <returns>True if the password meets strength requirements; otherwise, false.</returns>
    public static bool IsValidStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var rule = new PasswordStrengthRule(password);
        return rule.IsSatisfied();
    }

    /// <summary>
    /// Gets the validation error message for a weak password.
    /// </summary>
    /// <param name="password">The plain text password to validate.</param>
    /// <returns>The validation error message if weak; otherwise, null.</returns>
    public static string? GetValidationErrorMessage(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return ValidationConstants.PasswordEmptyErrorMessage;

        var rule = new PasswordStrengthRule(password);
        return rule.IsSatisfied() ? null : rule.Message;
    }
}
