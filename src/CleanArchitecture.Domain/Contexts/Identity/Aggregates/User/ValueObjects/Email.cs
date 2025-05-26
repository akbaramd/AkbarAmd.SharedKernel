using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (email is null)
            throw new ArgumentNullException(nameof(email));

        var trimmed = email.Trim();
        return new Email(trimmed.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        // Basic email regex; extend as needed
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email?.Value;

    // Override async validation hook
    public override  void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ArgumentException("Email cannot be empty or whitespace.", nameof(Value));

        if (!IsValidEmail(Value))
            throw new ArgumentException("Invalid email format.", nameof(Value));

    
    }
}
