using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

public class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");

        // Simple hashing for example purposes - In production use a proper password hasher
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToBase64String(hashedBytes);

        return new PasswordHash(hash);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
} 