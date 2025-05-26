using System;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Policies;

public class UserStatusPolicy
{
    public static bool CanDeactivate(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // User must be active to be deactivated
        if (user.Status != UserStatus.Active)
            return false;

        // Cannot deactivate deleted users
        if (user.Status == UserStatus.Deleted)
            return false;

        return true;
    }

    public static bool CanActivate(UserEntity user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // User must be inactive or suspended to be activated
        if (user.Status != UserStatus.Inactive && user.Status != UserStatus.Suspended)
            return false;

        // Cannot activate deleted users
        if (user.Status == UserStatus.Deleted)
            return false;

        return true;
    }

    public static string GetDeactivationErrorMessage(UserEntity user)
    {
        if (user == null)
            return "User cannot be null";

        if (user.Status != UserStatus.Active)
            return "User is not active";

        if (user.Status == UserStatus.Deleted)
            return "Cannot deactivate a deleted user";

        return string.Empty;
    }
} 