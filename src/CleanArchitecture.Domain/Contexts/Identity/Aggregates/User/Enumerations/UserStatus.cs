using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

public sealed class UserStatus : Enumeration
{
    public static readonly UserStatus Active = new(1, nameof(Active), "The user is active and can access the system.");
    public static readonly UserStatus Inactive = new(2, nameof(Inactive), "The user account is inactive.");
    public static readonly UserStatus Suspended = new(3, nameof(Suspended), "The user is suspended due to policy violations.");
    public static readonly UserStatus Deleted = new(4, nameof(Deleted), "The user account has been deleted.");

    private UserStatus(int id, string name, string? description = null) : base(id, name, description)
    {
    }
}
