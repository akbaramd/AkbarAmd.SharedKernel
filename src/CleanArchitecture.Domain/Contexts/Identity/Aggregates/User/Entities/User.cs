using System;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities
{
    /// <summary>
    /// Aggregate Root representing a User in the Identity context.
    /// Encapsulates user state, business rules, and domain events.
    /// </summary>
    public sealed class UserEntity : AggregateRoot<Guid>
    {
        public Email Email { get; private set; }
        public PasswordHash Password { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public UserStatus Status { get; private set; }

        /// <summary>
        /// Creates a new UserEntity aggregate.
        /// </summary>
        /// <param name="id">Unique identifier</param>
        /// <param name="email">User's email (value object)</param>
        /// <param name="password">Password hash (value object)</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal UserEntity(Guid id, Email email, PasswordHash password, string firstName, string lastName) : base(id)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            FirstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
            LastName = !string.IsNullOrWhiteSpace(lastName) ? lastName : throw new ArgumentException("LastName cannot be empty", nameof(lastName));
            Status = UserStatus.Active;

            AddDomainEvent(new UserCreatedEvent(id, email.Value));
        }

        /// <summary>
        /// Deactivates the user if allowed by policy.
        /// </summary>
        /// <param name="reason">Optional reason for deactivation</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Deactivate(string reason = null)
        {
            if (!UserStatusPolicy.CanDeactivate(this))
                throw new InvalidOperationException(UserStatusPolicy.GetDeactivationErrorMessage(this));

            Status = UserStatus.Inactive;
            AddDomainEvent(new UserDeactivatedEvent(Id, reason));
        }

        /// <summary>
        /// Activates the user if allowed by policy.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Activate()
        {
            if (!UserStatusPolicy.CanActivate(this))
                throw new InvalidOperationException("User is already active or cannot be activated.");

            Status = UserStatus.Active;
            AddDomainEvent(new UserActivatedEvent(Id));
        }

        /// <summary>
        /// Suspends the user if not deleted.
        /// </summary>
        /// <param name="reason">Reason for suspension</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Suspend(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason for suspension cannot be empty", nameof(reason));

            if (Status == UserStatus.Deleted)
                throw new InvalidOperationException("Cannot suspend a deleted user.");

            if (!UserStatusPolicy.CanSuspend(this))
                throw new InvalidOperationException(UserStatusPolicy.GetSuspensionErrorMessage(this));

            Status = UserStatus.Suspended;
            AddDomainEvent(new UserSuspendedEvent(Id, reason));
        }

        /// <summary>
        /// Deletes the user (soft delete).
        /// </summary>
        public void Delete()
        {
            if (Status == UserStatus.Deleted)
                throw new InvalidOperationException("User is already deleted.");

            Status = UserStatus.Deleted;
            AddDomainEvent(new UserDeletedEvent(Id));
        }

        /// <summary>
        /// Checks if the user is currently active.
        /// </summary>
        public bool IsActive => Status == UserStatus.Active;

        // Optional: Add update methods for FirstName, LastName, Password, Email with validation and events

        /// <summary>
        /// Updates the user's email.
        /// </summary>
        /// <param name="email">New email</param>
        public void UpdateEmail(Email email)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            if (Email.Equals(email)) return; // no change

            Email = email;
            AddDomainEvent(new UserEmailChangedEvent(Id, email.Value));
        }

        /// <summary>
        /// Updates the user's password.
        /// </summary>
        /// <param name="passwordHash">New password hash</param>
        public void UpdatePassword(PasswordHash passwordHash)
        {
            if (passwordHash == null) throw new ArgumentNullException(nameof(passwordHash));
            if (Password.Equals(passwordHash)) return; // no change

            Password = passwordHash;
            AddDomainEvent(new UserPasswordChangedEvent(Id));
        }

        /// <summary>
        /// Updates user's name.
        /// </summary>
        /// <param name="firstName">New first name</param>
        /// <param name="lastName">New last name</param>
        public void UpdateName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("FirstName cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("LastName cannot be empty", nameof(lastName));

            bool changed = false;

            if (!string.Equals(FirstName, firstName))
            {
                FirstName = firstName;
                changed = true;
            }

            if (!string.Equals(LastName, lastName))
            {
                LastName = lastName;
                changed = true;
            }

            if (changed)
            {
                AddDomainEvent(new UserNameChangedEvent(Id, firstName, lastName));
            }
        }
    }
}
