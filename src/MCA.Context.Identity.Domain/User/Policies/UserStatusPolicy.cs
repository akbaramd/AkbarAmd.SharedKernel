/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Policy: User Status Transition Policy
 * Year: 2025
 */

using MCA.Context.Identity.Domain.User.Constants;
using MCA.Context.Identity.Domain.User.Entities;
using MCA.Context.Identity.Domain.User.Enumerations;

namespace MCA.Context.Identity.Domain.User.Policies;

/// <summary>
/// Policy for managing user status transitions.
/// Defines the business rules for when users can change status.
/// </summary>
public static class UserStatusPolicy
{
    #region Status Transition Rules

    /// <summary>
    /// Determines if a user can be deactivated.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user can be deactivated; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static bool CanDeactivate(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Cannot deactivate unless user is active and not deleted
        if (user.Status != UserStatus.Active)
            return false;

        if (user.Status == UserStatus.Deleted)
            return false;

        return true;
    }

    /// <summary>
    /// Determines if a user can be activated.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user can be activated; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static bool CanActivate(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Only inactive or suspended users can be activated
        if (user.Status != UserStatus.Inactive && user.Status != UserStatus.Suspended)
            return false;

        if (user.Status == UserStatus.Deleted)
            return false;

        return true;
    }

    /// <summary>
    /// Determines if a user can be suspended.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user can be suspended; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static bool CanSuspend(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Cannot suspend deleted or inactive users
        if (user.Status == UserStatus.Deleted || user.Status == UserStatus.Inactive)
            return false;

        return true;
    }

    /// <summary>
    /// Determines if a user can be deleted.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user can be deleted; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static bool CanDelete(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Cannot delete already deleted users
        if (user.Status == UserStatus.Deleted)
            return false;

        return true;
    }

    #endregion

    #region Error Messages

    /// <summary>
    /// Gets error message for why deactivation is not allowed.
    /// Returns empty string if deactivation is allowed.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>Error message or empty string if deactivation is allowed.</returns>
    public static string GetDeactivationErrorMessage(UserEntity user)
    {
        if (user == null)
            return ValidationConstants.UserNullErrorMessage;

        if (user.Status == UserStatus.Deleted)
            return ValidationConstants.CannotDeactivateDeletedUserErrorMessage;

        if (user.Status != UserStatus.Active)
            return string.Format(ValidationConstants.CannotDeactivateUserWithStatusErrorMessage, user.Status.Name);

        return string.Empty;
    }

    /// <summary>
    /// Gets error message for why activation is not allowed.
    /// Returns empty string if activation is allowed.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>Error message or empty string if activation is allowed.</returns>
    public static string GetActivationErrorMessage(UserEntity user)
    {
        if (user == null)
            return ValidationConstants.UserNullErrorMessage;

        if (user.Status == UserStatus.Deleted)
            return ValidationConstants.CannotActivateDeletedUserErrorMessage;

        if (user.Status == UserStatus.Active)
            return ValidationConstants.UserAlreadyActiveErrorMessage;

        return string.Empty;
    }

    /// <summary>
    /// Gets error message for why suspension is not allowed.
    /// Returns empty string if suspension is allowed.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>Error message or empty string if suspension is allowed.</returns>
    public static string GetSuspensionErrorMessage(UserEntity user)
    {
        if (user == null)
            return ValidationConstants.UserNullErrorMessage;

        if (user.Status == UserStatus.Deleted)
            return ValidationConstants.CannotSuspendDeletedUserErrorMessage;

        if (user.Status == UserStatus.Inactive)
            return ValidationConstants.CannotSuspendInactiveUserErrorMessage;

        if (user.Status == UserStatus.Suspended)
            return ValidationConstants.UserAlreadySuspendedErrorMessage;

        return string.Empty;
    }

    /// <summary>
    /// Gets error message for why deletion is not allowed.
    /// Returns empty string if deletion is allowed.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>Error message or empty string if deletion is allowed.</returns>
    public static string GetDeletionErrorMessage(UserEntity user)
    {
        if (user == null)
            return ValidationConstants.UserNullErrorMessage;

        if (user.Status == UserStatus.Deleted)
            return ValidationConstants.UserAlreadyDeletedErrorMessage;

        return string.Empty;
    }

    #endregion

    #region Status Information

    /// <summary>
    /// Gets all valid status transitions for a user.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>Array of valid status transitions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static UserStatus[] GetValidTransitions(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return user.Status switch
        {
            UserStatus status when status == UserStatus.Active => new[] { UserStatus.Inactive, UserStatus.Suspended, UserStatus.Deleted },
            UserStatus status when status == UserStatus.Inactive => new[] { UserStatus.Active, UserStatus.Deleted },
            UserStatus status when status == UserStatus.Suspended => new[] { UserStatus.Active, UserStatus.Deleted },
            UserStatus status when status == UserStatus.Deleted => Array.Empty<UserStatus>(),
            _ => Array.Empty<UserStatus>()
        };
    }

    /// <summary>
    /// Determines if a status transition is valid.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="newStatus">The new status to transition to.</param>
    /// <returns>True if the transition is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when user is null.</exception>
    public static bool IsValidTransition(UserEntity user, UserStatus newStatus)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var validTransitions = GetValidTransitions(user);
        return validTransitions.Contains(newStatus);
    }

    #endregion
}
