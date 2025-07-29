/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Advanced User Factory with Domain-Driven Design Patterns
 * Year: 2025
 */

using MCA.Identity.Domain.User.Entities;
using MCA.Identity.Domain.User.Enumerations;
using MCA.Identity.Domain.User.Policies;
using MCA.Identity.Domain.User.ValueObjects;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Identity.Domain.User.Factories;

/// <summary>
/// Advanced factory for creating User entities with various creation scenarios.
/// Implements Domain-Driven Design patterns for complex object creation.
/// Handles default values, computed properties, and business rule validation.
/// </summary>
public static class UserFactory
{
    #region Basic User Creation

    /// <summary>
    /// Creates a new user with basic information and default settings.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password (will be validated).</param>
    /// <param name="firstName">User's first name.</param>
    /// <param name="lastName">User's last name.</param>
    /// <param name="createdBy">Identifier of the creator (defaults to "system").</param>
    /// <returns>New UserEntity instance with Active status.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static UserEntity CreateNewUser(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy = "system")
    {
        // Validate input parameters
        ValidateBasicUserInput(email, password, firstName, lastName, createdBy);

        // Create value objects with validation
        var emailVO = Email.Create(email);
        var passwordHash = Password.Create(password);

        // Create user with default settings
        return UserEntity.Create(
            Guid.NewGuid(),
            emailVO,
            passwordHash,
            firstName,
            lastName,
            createdBy);
    }

    /// <summary>
    /// Creates a new user with a specific ID (useful for testing or data migration).
    /// </summary>
    /// <param name="id">Specific user ID to assign.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password (will be validated).</param>
    /// <param name="firstName">User's first name.</param>
    /// <param name="lastName">User's last name.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>New UserEntity instance with the specified ID.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static UserEntity CreateUserWithId(
        Guid id,
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy = "system")
    {
        // Validate input parameters
        ValidateBasicUserInput(email, password, firstName, lastName, createdBy);
        ValidateUserId(id);

        // Create value objects with validation
        var emailVO = Email.Create(email);
        var passwordHash = Password.Create(password);

        // Create user with specific ID
        return UserEntity.Create(
            id,
            emailVO,
            passwordHash,
            firstName,
            lastName,
            createdBy);
    }

    #endregion

    #region Advanced User Creation

    /// <summary>
    /// Creates a new user with specific status and computed properties.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="password">User's password (will be validated).</param>
    /// <param name="firstName">User's first name.</param>
    /// <param name="lastName">User's last name.</param>
    /// <param name="status">Initial user status.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>New UserEntity instance with specified status.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static UserEntity CreateUserWithStatus(
        string email,
        string password,
        string firstName,
        string lastName,
        UserStatus status,
        string createdBy = "system")
    {
        // Validate input parameters
        ValidateBasicUserInput(email, password, firstName, lastName, createdBy);
        ValidateUserStatus(status);

        // Create value objects with validation
        var emailVO = Email.Create(email);
        var passwordHash = Password.Create(password);

        // Create user with specific status
        var user = UserEntity.Create(
            Guid.NewGuid(),
            emailVO,
            passwordHash,
            firstName,
            lastName,
            createdBy);

        // Apply status if different from default (Active)
        if (status != UserStatus.Active)
        {
            ApplyUserStatus(user, status);
        }

        return user;
    }

    /// <summary>
    /// Creates a system user with elevated privileges and specific settings.
    /// </summary>
    /// <param name="email">System user's email address.</param>
    /// <param name="password">System user's password (will be validated).</param>
    /// <param name="firstName">System user's first name.</param>
    /// <param name="lastName">System user's last name.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>New UserEntity instance configured as system user.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static UserEntity CreateSystemUser(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy = "system")
    {
        // Validate system user requirements
        ValidateSystemUserInput(email, password, firstName, lastName, createdBy);

        // Create value objects with enhanced validation for system users
        var emailVO = Email.Create(email);
        var passwordHash = Password.Create(password);

        // Create system user with specific settings
        var user = UserEntity.Create(
            Guid.NewGuid(),
            emailVO,
            passwordHash,
            firstName,
            lastName,
            createdBy);

        // System users are always active by default
        // Additional system-specific logic can be added here

        return user;
    }

    /// <summary>
    /// Creates a temporary user for testing or temporary access scenarios.
    /// </summary>
    /// <param name="email">Temporary user's email address.</param>
    /// <param name="password">Temporary user's password.</param>
    /// <param name="firstName">Temporary user's first name.</param>
    /// <param name="lastName">Temporary user's last name.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>New UserEntity instance configured as temporary user.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public static UserEntity CreateTemporaryUser(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy = "system")
    {
        // Validate temporary user requirements
        ValidateTemporaryUserInput(email, password, firstName, lastName, createdBy);

        // Create value objects with relaxed validation for temporary users
        var emailVO = Email.Create(email);
        var passwordHash = Password.Create(password);

        // Create temporary user
        var user = UserEntity.Create(
            Guid.NewGuid(),
            emailVO,
            passwordHash,
            firstName,
            lastName,
            createdBy);

        // Temporary users might have different default settings
        // Additional temporary user logic can be added here

        return user;
    }

    #endregion

    #region Bulk User Creation

    /// <summary>
    /// Creates multiple users from a collection of user data.
    /// </summary>
    /// <param name="userData">Collection of user creation data.</param>
    /// <param name="createdBy">Identifier of the creator.</param>
    /// <returns>Collection of created UserEntity instances.</returns>
    /// <exception cref="ArgumentException">Thrown when validation fails for any user.</exception>
    public static IEnumerable<UserEntity> CreateUsers(
        IEnumerable<UserCreationData> userData,
        string createdBy = "system")
    {
        if (userData == null) throw new ArgumentNullException(nameof(userData));
        if (createdBy == null) throw new ArgumentNullException(nameof(createdBy));

        var users = new List<UserEntity>();
        var errors = new List<string>();

        foreach (var data in userData)
        {
            try
            {
                var user = CreateNewUser(
                    data.Email,
                    data.Password,
                    data.FirstName,
                    data.LastName,
                    createdBy);
                users.Add(user);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to create user with email '{data.Email}': {ex.Message}");
            }
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Failed to create some users: {string.Join("; ", errors)}");
        }

        return users;
    }

    #endregion

    #region User Creation Data Structure

    /// <summary>
    /// Data structure for user creation parameters.
    /// Encapsulates user creation data and provides validation.
    /// </summary>
    public sealed record UserCreationData
    {
        public string Email { get; }
        public string Password { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public UserStatus? Status { get; }

        public UserCreationData(
            string email,
            string password,
            string firstName,
            string lastName,
            UserStatus? status = null)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Status = status;
        }

        /// <summary>
        /// Validates the user creation data.
        /// </summary>
        /// <returns>Validation result with any error messages.</returns>
        public ValidationResult Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email cannot be empty.");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password cannot be empty.");

            if (string.IsNullOrWhiteSpace(FirstName))
                errors.Add("First name cannot be empty.");

            if (string.IsNullOrWhiteSpace(LastName))
                errors.Add("Last name cannot be empty.");

            return new ValidationResult(errors.Count == 0, errors);
        }
    }

    /// <summary>
    /// Validation result for user creation data.
    /// </summary>
    public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors)
    {
        public string ErrorMessage => string.Join("; ", Errors);
    }

    #endregion

    #region Private Validation Methods

    /// <summary>
    /// Validates basic user input parameters.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <param name="firstName">First name to validate.</param>
    /// <param name="lastName">Last name to validate.</param>
    /// <param name="createdBy">Creator identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateBasicUserInput(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));

        // Validate password using business rules
        var passwordValidation = PasswordPolicy.Validate(password);
        if (!passwordValidation.IsValid)
            throw new ArgumentException(passwordValidation.ErrorMessage, nameof(password));
    }

    /// <summary>
    /// Validates user ID.
    /// </summary>
    /// <param name="id">User ID to validate.</param>
    /// <exception cref="ArgumentException">Thrown when ID is invalid.</exception>
    private static void ValidateUserId(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(id));
    }

    /// <summary>
    /// Validates user status.
    /// </summary>
    /// <param name="status">User status to validate.</param>
    /// <exception cref="ArgumentException">Thrown when status is invalid.</exception>
    private static void ValidateUserStatus(UserStatus status)
    {
        if (status == null)
            throw new ArgumentException("User status cannot be null.", nameof(status));
    }

    /// <summary>
    /// Validates system user input with enhanced requirements.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <param name="firstName">First name to validate.</param>
    /// <param name="lastName">Last name to validate.</param>
    /// <param name="createdBy">Creator identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateSystemUserInput(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy)
    {
        ValidateBasicUserInput(email, password, firstName, lastName, createdBy);

        // Additional system user validation
        if (!email.Contains("@"))
            throw new ArgumentException("System user email must be valid.", nameof(email));

        // Enhanced password validation for system users
        var passwordValidation = PasswordPolicy.Validate(password);
        if (!passwordValidation.IsValid)
            throw new ArgumentException($"System user password validation failed: {passwordValidation.ErrorMessage}", nameof(password));
    }

    /// <summary>
    /// Validates temporary user input with relaxed requirements.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <param name="firstName">First name to validate.</param>
    /// <param name="lastName">Last name to validate.</param>
    /// <param name="createdBy">Creator identifier to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateTemporaryUserInput(
        string email,
        string password,
        string firstName,
        string lastName,
        string createdBy)
    {
        // Basic validation for temporary users
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));

        // Relaxed password validation for temporary users
        if (password.Length < 6)
            throw new ArgumentException("Temporary user password must be at least 6 characters.", nameof(password));
    }

    /// <summary>
    /// Applies a specific status to a user entity.
    /// </summary>
    /// <param name="user">User entity to modify.</param>
    /// <param name="status">Status to apply.</param>
    private static void ApplyUserStatus(UserEntity user, UserStatus status)
    {
        if (status == UserStatus.Inactive)
        {
            user.Deactivate("Factory: Initial inactive status");
        }
        else if (status == UserStatus.Suspended)
        {
            user.Suspend("Factory: Initial suspended status");
        }
        else if (status == UserStatus.Deleted)
        {
            user.Delete();
        }
        else if (status == UserStatus.Active)
        {
            // Already active by default
        }
        else
        {
            // Unknown status - keep default active status
        }
    }

    #endregion
} 