using System;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies
{
    public static class UserStatusPolicy
    {
        /// <summary>
        /// Determines if a user can be deactivated.
        /// </summary>
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
        /// Gets error message for why deactivation is not allowed.
        /// Returns empty string if deactivation is allowed.
        /// </summary>
        public static string GetDeactivationErrorMessage(UserEntity user)
        {
            if (user == null)
                return "User cannot be null";

            if (user.Status == UserStatus.Deleted)
                return "Cannot deactivate a deleted user";

            if (user.Status != UserStatus.Active)
                return "User is not active";

            return string.Empty;
        }

        /// <summary>
        /// Gets error message for why suspension is not allowed.
        /// Returns empty string if suspension is allowed.
        /// </summary>
        public static string GetSuspensionErrorMessage(UserEntity user)
        {
            if (user == null)
                return "User cannot be null";

            if (user.Status == UserStatus.Deleted)
                return "Cannot suspend a deleted user";

            if (user.Status == UserStatus.Inactive)
                return "Cannot suspend an inactive user";

            return string.Empty;
        }
    }
}
