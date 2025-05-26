using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        // Simple email validation for example
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
} 