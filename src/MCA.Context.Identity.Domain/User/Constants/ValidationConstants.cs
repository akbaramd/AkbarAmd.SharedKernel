/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Validation Constants for User Domain
 * Year: 2025
 */

namespace MCA.Identity.Domain.User.Constants;

/// <summary>
/// Constants for validation rules in the User domain.
/// These constants define the business rules for email, password, name validation, and user status transitions.
/// </summary>
public static class ValidationConstants
{
    #region Email Validation

    /// <summary>
    /// Maximum length for email addresses.
    /// </summary>
    public const int EmailMaxLength = 254; // RFC 5321

    /// <summary>
    /// Minimum length for email addresses.
    /// </summary>
    public const int EmailMinLength = 5; // a@b.c

    /// <summary>
    /// Regular expression pattern for email validation.
    /// </summary>
    public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    /// <summary>
    /// Error message for invalid email format.
    /// </summary>
    public const string EmailFormatErrorMessage = "Email must be in a valid format (e.g., user@domain.com)";

    /// <summary>
    /// Error message for email that is too short.
    /// </summary>
    public const string EmailTooShortErrorMessage = "Email must be at least {0} characters long";

    /// <summary>
    /// Error message for email that is too long.
    /// </summary>
    public const string EmailTooLongErrorMessage = "Email cannot exceed {0} characters";

    /// <summary>
    /// Error message for empty email.
    /// </summary>
    public const string EmailEmptyErrorMessage = "Email cannot be empty or whitespace";

    #endregion

    #region Password Validation

    /// <summary>
    /// Minimum length for passwords.
    /// </summary>
    public const int PasswordMinLength = 8;

    /// <summary>
    /// Maximum length for passwords.
    /// </summary>
    public const int PasswordMaxLength = 128;

    /// <summary>
    /// Minimum number of uppercase letters required in password.
    /// </summary>
    public const int PasswordMinUppercase = 1;

    /// <summary>
    /// Minimum number of lowercase letters required in password.
    /// </summary>
    public const int PasswordMinLowercase = 1;

    /// <summary>
    /// Minimum number of digits required in password.
    /// </summary>
    public const int PasswordMinDigits = 1;

    /// <summary>
    /// Minimum number of special characters required in password.
    /// </summary>
    public const int PasswordMinSpecialChars = 1;

    /// <summary>
    /// Regular expression pattern for password validation.
    /// </summary>
    public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";

    /// <summary>
    /// Error message for password that is too short.
    /// </summary>
    public const string PasswordTooShortErrorMessage = "Password must be at least {0} characters long";

    /// <summary>
    /// Error message for password that is too long.
    /// </summary>
    public const string PasswordTooLongErrorMessage = "Password cannot exceed {0} characters";

    /// <summary>
    /// Error message for password missing uppercase letters.
    /// </summary>
    public const string PasswordMissingUppercaseErrorMessage = "Password must contain at least {0} uppercase letter(s)";

    /// <summary>
    /// Error message for password missing lowercase letters.
    /// </summary>
    public const string PasswordMissingLowercaseErrorMessage = "Password must contain at least {0} lowercase letter(s)";

    /// <summary>
    /// Error message for password missing digits.
    /// </summary>
    public const string PasswordMissingDigitsErrorMessage = "Password must contain at least {0} digit(s)";

    /// <summary>
    /// Error message for password missing special characters.
    /// </summary>
    public const string PasswordMissingSpecialCharsErrorMessage = "Password must contain at least {0} special character(s)";

    /// <summary>
    /// Error message for empty password.
    /// </summary>
    public const string PasswordEmptyErrorMessage = "Password cannot be empty or whitespace";

    /// <summary>
    /// Error message for weak password.
    /// </summary>
    public const string PasswordWeakErrorMessage = "Password does not meet security requirements";

    #endregion

    #region Name Validation

    /// <summary>
    /// Minimum length for first name.
    /// </summary>
    public const int FirstNameMinLength = 2;

    /// <summary>
    /// Maximum length for first name.
    /// </summary>
    public const int FirstNameMaxLength = 50;

    /// <summary>
    /// Minimum length for last name.
    /// </summary>
    public const int LastNameMinLength = 2;

    /// <summary>
    /// Maximum length for last name.
    /// </summary>
    public const int LastNameMaxLength = 50;

    /// <summary>
    /// Regular expression pattern for name validation (letters, spaces, hyphens, apostrophes).
    /// </summary>
    public const string NamePattern = @"^[a-zA-Z\s\-']+$";

    /// <summary>
    /// Error message for first name that is too short.
    /// </summary>
    public const string FirstNameTooShortErrorMessage = "First name must be at least {0} characters long";

    /// <summary>
    /// Error message for first name that is too long.
    /// </summary>
    public const string FirstNameTooLongErrorMessage = "First name cannot exceed {0} characters";

    /// <summary>
    /// Error message for last name that is too short.
    /// </summary>
    public const string LastNameTooShortErrorMessage = "Last name must be at least {0} characters long";

    /// <summary>
    /// Error message for last name that is too long.
    /// </summary>
    public const string LastNameTooLongErrorMessage = "Last name cannot exceed {0} characters";

    /// <summary>
    /// Error message for invalid name format.
    /// </summary>
    public const string NameInvalidFormatErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes";

    /// <summary>
    /// Error message for empty name.
    /// </summary>
    public const string NameEmptyErrorMessage = "Name cannot be empty or whitespace";

    #endregion

    #region User Status Validation

    /// <summary>
    /// Error message for null user.
    /// </summary>
    public const string UserNullErrorMessage = "User cannot be null";

    /// <summary>
    /// Error message for user already deleted.
    /// </summary>
    public const string UserAlreadyDeletedErrorMessage = "User is already deleted";

    /// <summary>
    /// Error message for user already active.
    /// </summary>
    public const string UserAlreadyActiveErrorMessage = "User is already active";

    /// <summary>
    /// Error message for user already suspended.
    /// </summary>
    public const string UserAlreadySuspendedErrorMessage = "User is already suspended";

    /// <summary>
    /// Error message for cannot deactivate deleted user.
    /// </summary>
    public const string CannotDeactivateDeletedUserErrorMessage = "Cannot deactivate a deleted user";

    /// <summary>
    /// Error message for cannot activate deleted user.
    /// </summary>
    public const string CannotActivateDeletedUserErrorMessage = "Cannot activate a deleted user";

    /// <summary>
    /// Error message for cannot suspend deleted user.
    /// </summary>
    public const string CannotSuspendDeletedUserErrorMessage = "Cannot suspend a deleted user";

    /// <summary>
    /// Error message for cannot suspend inactive user.
    /// </summary>
    public const string CannotSuspendInactiveUserErrorMessage = "Cannot suspend an inactive user";

    /// <summary>
    /// Error message for cannot deactivate user with specific status.
    /// </summary>
    public const string CannotDeactivateUserWithStatusErrorMessage = "Cannot deactivate a user with status '{0}'";

    #endregion

    #region Suspension Validation

    /// <summary>
    /// Minimum length for suspension reason.
    /// </summary>
    public const int SuspensionReasonMinLength = 10;

    /// <summary>
    /// Maximum length for suspension reason.
    /// </summary>
    public const int SuspensionReasonMaxLength = 500;

    /// <summary>
    /// Error message for empty suspension reason.
    /// </summary>
    public const string SuspensionReasonEmptyErrorMessage = "Suspension reason is required";

    /// <summary>
    /// Error message for suspension reason that is too short.
    /// </summary>
    public const string SuspensionReasonTooShortErrorMessage = "Suspension reason must be at least {0} characters long";

    /// <summary>
    /// Error message for suspension reason that is too long.
    /// </summary>
    public const string SuspensionReasonTooLongErrorMessage = "Suspension reason cannot exceed {0} characters";

    #endregion

    #region Common Validation

    /// <summary>
    /// Error message for null value.
    /// </summary>
    public const string NullValueErrorMessage = "{0} cannot be null";

    /// <summary>
    /// Error message for empty string.
    /// </summary>
    public const string EmptyStringErrorMessage = "{0} cannot be empty";

    /// <summary>
    /// Error message for whitespace string.
    /// </summary>
    public const string WhitespaceStringErrorMessage = "{0} cannot be whitespace";

    /// <summary>
    /// Error message for invalid operation.
    /// </summary>
    public const string InvalidOperationErrorMessage = "Invalid operation: {0}";

    /// <summary>
    /// Error message for validation failure.
    /// </summary>
    public const string ValidationFailureErrorMessage = "Validation failed: {0}";

    #endregion

    #region Business Rule Messages

    /// <summary>
    /// Error message for business rule violation.
    /// </summary>
    public const string BusinessRuleViolationErrorMessage = "Business rule violation: {0}";

    /// <summary>
    /// Error message for domain rule violation.
    /// </summary>
    public const string DomainRuleViolationErrorMessage = "Domain rule violation: {0}";

    /// <summary>
    /// Error message for invariant violation.
    /// </summary>
    public const string InvariantViolationErrorMessage = "Invariant violation: {0}";

    #endregion

    #region Security Constants

    /// <summary>
    /// Minimum password strength score (0-100).
    /// </summary>
    public const int MinimumPasswordStrengthScore = 70;

    /// <summary>
    /// Maximum login attempts before account lockout.
    /// </summary>
    public const int MaxLoginAttempts = 5;

    /// <summary>
    /// Account lockout duration in minutes.
    /// </summary>
    public const int AccountLockoutDurationMinutes = 30;

    /// <summary>
    /// Password change required after days.
    /// </summary>
    public const int PasswordChangeRequiredDays = 90;

    /// <summary>
    /// Session timeout in minutes.
    /// </summary>
    public const int SessionTimeoutMinutes = 60;

    #endregion

    #region Cache Constants

    /// <summary>
    /// Default cache duration for user data in minutes.
    /// </summary>
    public const int DefaultUserCacheDurationMinutes = 30;

    /// <summary>
    /// Cache duration for user profile in minutes.
    /// </summary>
    public const int UserProfileCacheDurationMinutes = 60;

    /// <summary>
    /// Cache duration for user permissions in minutes.
    /// </summary>
    public const int UserPermissionsCacheDurationMinutes = 15;

    #endregion
} 