using System.Security.Cryptography;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100_000; // Work factor

    public string Value { get; }  // Format: base64(salt):base64(hash)

    private PasswordHash(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static PasswordHash Create(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var trimmed = password.Trim();

        var instance = new PasswordHash(HashPassword(trimmed));

        // Optionally validate synchronously here if needed:
        // instance.ValidateSync();

        return instance;
    }

   

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
    /// Verify if the provided password matches this hash.
    /// </summary>
    public bool Verify(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var parts = Value.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHash = Convert.FromBase64String(parts[1]);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => "****"; // Never reveal hash string accidentally

    public override void Validate()
    {
        // Perform all validation here asynchronously if needed

        // Synchronous validation logic (can be refactored into a method)
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

        // Example: add async validation if you want, e.g., check against compromised passwords service
        Validate();
    }
}
