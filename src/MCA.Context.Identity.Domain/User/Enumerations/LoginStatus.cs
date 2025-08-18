using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Enumerations;

/// <summary>
/// Represents the status of a user login in the identity system.
/// </summary>
public sealed class LoginStatus : Enumeration
{
    /// <summary>
    /// Login is registered but not yet used.
    /// </summary>
    public static readonly LoginStatus Registered = new(1, nameof(Registered), "Login is registered but not yet used.");

    /// <summary>
    /// Login is active and can be used.
    /// </summary>
    public static readonly LoginStatus Active = new(2, nameof(Active), "Login is active and can be used.");

    /// <summary>
    /// Login has failed and may be temporarily blocked.
    /// </summary>
    public static readonly LoginStatus Failed = new(3, nameof(Failed), "Login has failed and may be temporarily blocked.");

    /// <summary>
    /// Login is disabled and cannot be used.
    /// </summary>
    public static readonly LoginStatus Disabled = new(4, nameof(Disabled), "Login is disabled and cannot be used.");

    /// <summary>
    /// Login is suspended due to security concerns.
    /// </summary>
    public static readonly LoginStatus Suspended = new(5, nameof(Suspended), "Login is suspended due to security concerns.");

    /// <summary>
    /// Initializes a new instance of the LoginStatus enumeration.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    private LoginStatus(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 