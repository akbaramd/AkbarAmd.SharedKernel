/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Entity: User Claim
 * Year: 2025
 */

using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace MCA.Context.Identity.Domain.User.Entities;

/// <summary>
/// Represents a claim associated with a user in the identity system.
/// UserClaim is an entity that belongs to the User aggregate root.
/// </summary>
public sealed class UserClaimEntity : IdentityUserClaim<Guid>, IEntity<Guid>
{
    #region Properties

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the claim type.
    /// </summary>
    public string ClaimType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the claim value.
    /// </summary>
    public string ClaimValue { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the claim description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the claim is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the claim source (system, user, admin, etc.).
    /// </summary>
    public string Source { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; private set; }

    /// <summary>
    /// Gets the user who created this claim.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user who last modified this claim.
    /// </summary>
    public string LastModifiedBy { get; private set; } = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserClaimEntity() { }

    /// <summary>
    /// Creates a new user claim with the specified parameters.
    /// </summary>
    /// <param name="id">The claim ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="claimType">The claim type.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <param name="description">The claim description.</param>
    /// <param name="source">The claim source.</param>
    /// <param name="createdBy">The user creating the claim.</param>
    private UserClaimEntity(
        Guid id,
        Guid userId,
        string claimType,
        string claimValue,
        string description,
        string source,
        string createdBy)
        : base(id)
    {
        UserId = userId;
        ClaimType = claimType;
        ClaimValue = claimValue;
        Description = description;
        Source = source;
        IsActive = true;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = createdBy;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new user claim.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="claimType">The claim type.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <param name="description">The claim description.</param>
    /// <param name="source">The claim source.</param>
    /// <param name="createdBy">The user creating the claim.</param>
    /// <returns>A new user claim instance.</returns>
    public static UserClaimEntity Create(
        Guid userId,
        string claimType,
        string claimValue,
        string description = "",
        string source = "system",
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));
        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        var claim = new UserClaimEntity(
            Guid.NewGuid(),
            userId,
            claimType.Trim(),
            claimValue.Trim(),
            description?.Trim() ?? string.Empty,
            source.Trim(),
            createdBy);

        return claim;
    }

    /// <summary>
    /// Creates a new user claim with a specific ID.
    /// </summary>
    /// <param name="id">The claim ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="claimType">The claim type.</param>
    /// <param name="claimValue">The claim value.</param>
    /// <param name="description">The claim description.</param>
    /// <param name="source">The claim source.</param>
    /// <param name="createdBy">The user creating the claim.</param>
    /// <returns>A new user claim instance.</returns>
    public static UserClaimEntity CreateWithId(
        Guid id,
        Guid userId,
        string claimType,
        string claimValue,
        string description = "",
        string source = "system",
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));
        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        var claim = new UserClaimEntity(
            id,
            userId,
            claimType.Trim(),
            claimValue.Trim(),
            description?.Trim() ?? string.Empty,
            source.Trim(),
            createdBy);

        return claim;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Updates the claim information.
    /// </summary>
    /// <param name="claimType">The new claim type.</param>
    /// <param name="claimValue">The new claim value.</param>
    /// <param name="description">The new claim description.</param>
    /// <param name="modifiedBy">The user modifying the claim.</param>
    public void Update(
        string claimType,
        string claimValue,
        string description,
        string modifiedBy)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update inactive claim");

        // Validate input
        if (string.IsNullOrWhiteSpace(claimType))
            throw new ArgumentException("Claim type cannot be empty", nameof(claimType));
        if (string.IsNullOrWhiteSpace(claimValue))
            throw new ArgumentException("Claim value cannot be empty", nameof(claimValue));

        ClaimType = claimType.Trim();
        ClaimValue = claimValue.Trim();
        Description = description?.Trim() ?? string.Empty;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Activates the claim.
    /// </summary>
    /// <param name="activatedBy">The user activating the claim.</param>
    public void Activate(string activatedBy)
    {
        if (IsActive)
            throw new InvalidOperationException("Claim is already active");

        IsActive = true;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = activatedBy;
    }

    /// <summary>
    /// Deactivates the claim.
    /// </summary>
    /// <param name="deactivatedBy">The user deactivating the claim.</param>
    public void Deactivate(string deactivatedBy)
    {
        if (!IsActive)
            throw new InvalidOperationException("Claim is already inactive");

        IsActive = false;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = deactivatedBy;
    }

    /// <summary>
    /// Checks if the claim is active.
    /// </summary>
    /// <returns>True if the claim is active; otherwise, false.</returns>
    public bool IsActiveClaim() => IsActive;

    /// <summary>
    /// Checks if the claim is system-generated.
    /// </summary>
    /// <returns>True if the claim is system-generated; otherwise, false.</returns>
    public bool IsSystemClaim() => Source.Equals("system", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if the claim can be modified.
    /// </summary>
    /// <returns>True if the claim can be modified; otherwise, false.</returns>
    public bool CanBeModified() => IsActive;

    #endregion
} 