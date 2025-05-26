using System;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Factories;

public static class UserFactory
{
    public static UserEntity CreateNewUser(
        string email,
        string password,
        string firstName,
        string lastName)
    {
        var passwordValidation = PasswordPolicy.Validate(password);
        if (!passwordValidation.IsValid)
            throw new ArgumentException(passwordValidation.ErrorMessage, nameof(password));

        var emailVO = Email.Create(email);
        var passwordHash = PasswordHash.Create(password);

        return new UserEntity(
            Guid.NewGuid(),
            emailVO,
            passwordHash,
            firstName,
            lastName);
    }
} 