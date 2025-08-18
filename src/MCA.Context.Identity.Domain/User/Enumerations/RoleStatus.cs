using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Enumerations;

/// <summary>
/// Represents the status of a role in the identity system.
/// </summary>
public sealed class RoleStatus : Enumeration
{
    /// <summary>
    /// Role is active and can be assigned to users.
    /// </summary>
    public static readonly RoleStatus Active = new(1, nameof(Active), "Role is active and can be assigned to users.");

    /// <summary>
    /// Role is inactive and cannot be assigned to users.
    /// </summary>
    public static readonly RoleStatus Inactive = new(2, nameof(Inactive), "Role is inactive and cannot be assigned to users.");

    /// <summary>
    /// Role is deleted and cannot be used.
    /// </summary>
    public static readonly RoleStatus Deleted = new(3, nameof(Deleted), "Role is deleted and cannot be used.");

    /// <summary>
    /// Initializes a new instance of the RoleStatus enumeration.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    private RoleStatus(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 