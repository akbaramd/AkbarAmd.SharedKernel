using MCA.SharedKernel.Domain;

namespace MCA.Context.Identity.Domain.User.Enumerations;

/// <summary>
/// Represents the status of a token in the identity system.
/// </summary>
public sealed class TokenStatus : Enumeration
{
    /// <summary>
    /// Token is active and can be used.
    /// </summary>
    public static readonly TokenStatus Active = new(1, nameof(Active), "Token is active and can be used.");

    /// <summary>
    /// Token has been revoked and cannot be used.
    /// </summary>
    public static readonly TokenStatus Revoked = new(2, nameof(Revoked), "Token has been revoked and cannot be used.");

    /// <summary>
    /// Token has expired and cannot be used.
    /// </summary>
    public static readonly TokenStatus Expired = new(3, nameof(Expired), "Token has expired and cannot be used.");

    /// <summary>
    /// Token is suspended due to security concerns.
    /// </summary>
    public static readonly TokenStatus Suspended = new(4, nameof(Suspended), "Token is suspended due to security concerns.");

    /// <summary>
    /// Token is pending activation.
    /// </summary>
    public static readonly TokenStatus Pending = new(5, nameof(Pending), "Token is pending activation.");

    /// <summary>
    /// Token is being processed.
    /// </summary>
    public static readonly TokenStatus Processing = new(6, nameof(Processing), "Token is being processed.");

    /// <summary>
    /// Token has been used and is no longer valid.
    /// </summary>
    public static readonly TokenStatus Used = new(7, nameof(Used), "Token has been used and is no longer valid.");

    /// <summary>
    /// Token has been compromised and is invalid.
    /// </summary>
    public static readonly TokenStatus Compromised = new(8, nameof(Compromised), "Token has been compromised and is invalid.");

    /// <summary>
    /// Initializes a new instance of the TokenStatus enumeration.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    private TokenStatus(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 