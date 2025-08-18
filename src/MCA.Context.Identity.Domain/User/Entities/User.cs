/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Aggregate Root: UserEntity
 * Year: 2025
 */

using MCA.Context.Identity.Domain.User.BusinessRules;
using MCA.Context.Identity.Domain.User.Enumerations;
using MCA.Context.Identity.Domain.User.Events;
using MCA.Context.Identity.Domain.User.Policies;
using MCA.Context.Identity.Domain.User.ValueObjects;
using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Entities;

/// <summary>
/// Aggregate Root representing an application user.
/// Implements full domain-event lifecycle, optimistic concurrency,
/// snapshotting, and policy-based state transitions.
/// Follows professional patterns and inherits from AggregateRoot for consistency.
/// </summary>
public sealed class UserEntity : AggregateRoot<Guid>
{
    #region State (backed by value objects / enumeration)

    /// <summary>
    /// User's email address as a value object.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// User's password as a value object.
    /// </summary>
    public Password Password { get; private set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Current status of the user.
    /// </summary>
    public UserStatus Status { get; private set; }

    #endregion

    #region Construction / Factory

    /// <summary>
    /// Private constructor for ORM support.
    /// </summary>
    private UserEntity() { }

    /// <summary>
    /// Private constructor for creating new user instances.
    /// </summary>
    /// <param name="id">Unique identifier for the user.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password.</param>
    /// <param name="firstName">User's first name.</param>
    /// <param name="lastName">User's last name.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    private UserEntity(
        Guid id,
        Email email,
        Password password,
        string firstName,
        string lastName,
        string createdBy = "system")
        : base(id, createdBy)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        
        // Validate names using business rules
        CheckRule(NameFormatRule.ForFirstName(firstName));
        CheckRule(NameFormatRule.ForLastName(lastName));
        
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Active;

        // Raise domain event using base class method
        RaiseEvent(new UserCreatedEvent(Id, Email.Value), _ => { });
    }

    /// <summary>
    /// Factory method for creating new user instances.
    /// </summary>
    /// <param name="id">Unique identifier for the user.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password.</param>
    /// <param name="firstName">User's first name.</param>
    /// <param name="lastName">User's last name.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>New UserEntity instance.</returns>
    public static UserEntity Create(
        Guid id,
        Email email,
        Password password,
        string firstName,
        string lastName,
        string createdBy = "system") =>
        new(id, email, password, firstName, lastName, createdBy);

    #endregion

    #region Business Logic / API

    /// <summary>
    /// Indicates whether the user is currently active.
    /// </summary>
    public bool IsActive => Status == UserStatus.Active;

    /// <summary>
    /// Activates the user if allowed by business rules.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when user cannot be activated.</exception>
    public void Activate()
    {
        if (!UserStatusPolicy.CanActivate(this))
            throw new InvalidOperationException("User cannot be activated.");

        RaiseEvent(new UserActivatedEvent(Id), _ => Status = UserStatus.Active);
    }

    /// <summary>
    /// Deactivates the user if allowed by business rules.
    /// </summary>
    /// <param name="reason">Optional reason for deactivation.</param>
    /// <exception cref="InvalidOperationException">Thrown when user cannot be deactivated.</exception>
    public void Deactivate(string? reason = null)
    {
        if (!UserStatusPolicy.CanDeactivate(this))
            throw new InvalidOperationException(UserStatusPolicy.GetDeactivationErrorMessage(this));

        RaiseEvent(new UserDeactivatedEvent(Id, reason), _ => Status = UserStatus.Inactive);
    }

    /// <summary>
    /// Suspends the user if allowed by business rules.
    /// </summary>
    /// <param name="reason">Required reason for suspension.</param>
    /// <exception cref="ArgumentException">Thrown when reason is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when user cannot be suspended.</exception>
    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Suspension reason required.", nameof(reason));
        if (!UserStatusPolicy.CanSuspend(this))
            throw new InvalidOperationException(UserStatusPolicy.GetSuspensionErrorMessage(this));

        RaiseEvent(new UserSuspendedEvent(Id, reason), _ => Status = UserStatus.Suspended);
    }

    /// <summary>
    /// Marks the user as deleted.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when user is already deleted.</exception>
    public void Delete()
    {
        if (Status == UserStatus.Deleted)
            throw new InvalidOperationException("User already deleted.");

        RaiseEvent(new UserDeletedEvent(Id), _ => Status = UserStatus.Deleted);
    }

    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    /// <param name="email">New email address.</param>
    /// <exception cref="ArgumentNullException">Thrown when email is null.</exception>
    public void UpdateEmail(Email email)
    {
        if (email is null) throw new ArgumentNullException(nameof(email));
        if (Email.Equals(email)) return;

        RaiseEvent(new UserEmailChangedEvent(Id, email.Value), _ => Email = email);
    }

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    /// <param name="password">New password.</param>
    /// <exception cref="ArgumentNullException">Thrown when password is null.</exception>
    public void UpdatePassword(Password password)
    {
        if (password is null) throw new ArgumentNullException(nameof(password));
        if (Password.Equals(password)) return;

        RaiseEvent(new UserPasswordChangedEvent(Id), _ => Password = password);
    }

    /// <summary>
    /// Updates the user's name.
    /// </summary>
    /// <param name="firstName">New first name.</param>
    /// <param name="lastName">New last name.</param>
    /// <exception cref="ArgumentException">Thrown when names are null or empty.</exception>
    public void UpdateName(string firstName, string lastName)
    {
        // Validate names using business rules
        CheckRule(NameFormatRule.ForFirstName(firstName));
        CheckRule(NameFormatRule.ForLastName(lastName));

        bool changed = false;
        string oldFirstName = FirstName;
        string oldLastName = LastName;

        if (!FirstName.Equals(firstName)) { FirstName = firstName; changed = true; }
        if (!LastName.Equals(lastName)) { LastName = lastName; changed = true; }

        if (changed)
        {
            RaiseEvent(new UserNameChangedEvent(Id, firstName, lastName), _ => { });
        }
    }

    #endregion

    #region Snapshotting

    /// <summary>
    /// Snapshot record for user state persistence.
    /// </summary>
    private sealed record UserSnapshot(
        Guid Id,
        string Email,
        string PasswordHash,
        string FirstName,
        string LastName,
        int Status,
        long Version,
        DateTimeOffset LastModifiedUtc);

    /// <summary>
    /// Creates a snapshot representing the current state of the user.
    /// </summary>
    /// <returns>Snapshot object containing user state.</returns>
    public override object CreateSnapshot()
    {
        SnapshotVersion++;
        return new UserSnapshot(
            Id,
            Email.Value,
            Password.Value,
            FirstName,
            LastName,
            Status.Id,
            Version,
            LastModifiedUtc);
    }

    /// <summary>
    /// Restores the user's state from the given snapshot.
    /// </summary>
    /// <param name="snapshot">Snapshot object to restore from.</param>
    /// <exception cref="ArgumentException">Thrown when snapshot is invalid or incompatible.</exception>
    public override void RestoreFromSnapshot(object snapshot)
    {
        if (snapshot is not UserSnapshot s)
            throw new ArgumentException("Invalid snapshot instance.", nameof(snapshot));

        Id = s.Id;
        Email = Email.Create(s.Email);
        Password = Password.FromHash(s.PasswordHash);
        FirstName = s.FirstName;
        LastName = s.LastName;
        Status = Enumeration.FromValue<UserStatus>(s.Status);
        // Note: Version and LastModifiedUtc are managed by base class

        OnRehydrated();
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified object is equal to the current user.
    /// </summary>
    /// <param name="obj">The object to compare with the current user.</param>
    /// <returns>True if the specified object is equal to the current user; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is UserEntity other && Equals(other);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current user.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Email, Password, FirstName, LastName, Status, Version, SnapshotVersion, LastModifiedUtc);
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Returns a string representation of the user.
    /// </summary>
    /// <returns>A string that represents the current user.</returns>
    public override string ToString() => $"{GetType().Name} [Id={Id}, Email={Email.Value}, Status={Status.Name}]";

    #endregion
}
