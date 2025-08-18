using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Enumerations;

/// <summary>
/// Represents the type of a role in the identity system.
/// </summary>
public sealed class RoleType : Enumeration
{
    /// <summary>
    /// System role with administrative privileges.
    /// </summary>
    public static readonly RoleType System = new(1, nameof(System), "System role with administrative privileges.");

    /// <summary>
    /// Administrative role with high privileges.
    /// </summary>
    public static readonly RoleType Administrator = new(2, nameof(Administrator), "Administrative role with high privileges.");

    /// <summary>
    /// Manager role with moderate privileges.
    /// </summary>
    public static readonly RoleType Manager = new(3, nameof(Manager), "Manager role with moderate privileges.");

    /// <summary>
    /// User role with basic privileges.
    /// </summary>
    public static readonly RoleType User = new(4, nameof(User), "User role with basic privileges.");

    /// <summary>
    /// Guest role with limited privileges.
    /// </summary>
    public static readonly RoleType Guest = new(5, nameof(Guest), "Guest role with limited privileges.");

    /// <summary>
    /// Custom role with specific privileges.
    /// </summary>
    public static readonly RoleType Custom = new(6, nameof(Custom), "Custom role with specific privileges.");

    /// <summary>
    /// Initializes a new instance of the RoleType enumeration.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    private RoleType(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 