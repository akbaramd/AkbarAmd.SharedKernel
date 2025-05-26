using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Enumerations;

public class UserStatus : Enumeration
{
    public static UserStatus Active = new(1, nameof(Active));
    public static UserStatus Inactive = new(2, nameof(Inactive));
    public static UserStatus Suspended = new(3, nameof(Suspended));
    public static UserStatus Deleted = new(4, nameof(Deleted));

    private UserStatus(int id, string name) : base(id, name)
    {
    }
} 