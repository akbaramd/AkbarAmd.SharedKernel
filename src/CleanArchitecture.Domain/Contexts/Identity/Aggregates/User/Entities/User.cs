using System;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;

public class UserEntity : AggregateRoot
{
    public Email Email { get; private set; }
    public PasswordHash Password { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserStatus Status { get; private set; }

    internal UserEntity(Guid id, Email email, PasswordHash password, string firstName, string lastName) : base(id)
    {
        Email = email;
        Password = password;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Active;

        AddDomainEvent(new UserCreatedEvent(id, email.Value));
    }

    public void Deactivate(string reason = null)
    {
        if (!UserStatusPolicy.CanDeactivate(this))
            throw new InvalidOperationException(UserStatusPolicy.GetDeactivationErrorMessage(this));
            
        Status = UserStatus.Inactive;
        AddDomainEvent(new UserDeactivatedEvent(Id, reason));
    }

    public void Activate()
    {
        if (!UserStatusPolicy.CanActivate(this))
            throw new InvalidOperationException("User is already active");
            
        Status = UserStatus.Active;
        AddDomainEvent(new UserActivatedEvent(Id));
    }

    public void Suspend(string reason)
    {
        if (Status == UserStatus.Deleted)
            throw new InvalidOperationException("Cannot suspend a deleted user");

        Status = UserStatus.Suspended;
        AddDomainEvent(new UserSuspendedEvent(Id, reason));
    }

    public void Delete()
    {
        Status = UserStatus.Deleted;
        AddDomainEvent(new UserDeletedEvent(Id));
    }

    public bool IsActive => Status == UserStatus.Active;
} 