/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Entity: User Token
 * Year: 2025
 */

using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;
using MCA.Context.Identity.Domain.User.Enumerations;

namespace MCA.Context.Identity.Domain.User.Entities;

/// <summary>
/// Represents a token associated with a user in the identity system.
/// UserToken is an entity that belongs to the User aggregate root.
/// </summary>
public sealed class UserTokenEntity : Entity<Guid>
{
    #region Properties

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the login provider name.
    /// </summary>
    public string LoginProvider { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the token name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the token value.
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the token type.
    /// </summary>
    public TokenType Type { get; private set; }

    /// <summary>
    /// Gets the token status.
    /// </summary>
    public TokenStatus Status { get; private set; }

    /// <summary>
    /// Gets the token expiration timestamp.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the token issued timestamp.
    /// </summary>
    public DateTimeOffset IssuedAt { get; private set; }

    /// <summary>
    /// Gets the token last used timestamp.
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; private set; }

    /// <summary>
    /// Gets the token metadata as JSON string.
    /// </summary>
    public string Metadata { get; private set; } = "{}";

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; private set; }

    /// <summary>
    /// Gets the user who created this token.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user who last modified this token.
    /// </summary>
    public string LastModifiedBy { get; private set; } = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserTokenEntity() { }

    /// <summary>
    /// Creates a new user token with the specified parameters.
    /// </summary>
    /// <param name="id">The token ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="name">The token name.</param>
    /// <param name="value">The token value.</param>
    /// <param name="type">The token type.</param>
    /// <param name="expiresAt">The token expiration timestamp.</param>
    /// <param name="metadata">The token metadata.</param>
    /// <param name="createdBy">The user creating the token.</param>
    private UserTokenEntity(
        Guid id,
        Guid userId,
        string loginProvider,
        string name,
        string value,
        TokenType type,
        DateTimeOffset? expiresAt,
        string metadata,
        string createdBy)
        : base(id)
    {
        UserId = userId;
        LoginProvider = loginProvider;
        Name = name;
        Value = value;
        Type = type;
        Status = TokenStatus.Active;
        ExpiresAt = expiresAt;
        IssuedAt = DateTimeOffset.UtcNow;
        Metadata = metadata;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = createdBy;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new user token.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="name">The token name.</param>
    /// <param name="value">The token value.</param>
    /// <param name="type">The token type.</param>
    /// <param name="expiresAt">The token expiration timestamp.</param>
    /// <param name="metadata">The token metadata.</param>
    /// <param name="createdBy">The user creating the token.</param>
    /// <returns>A new user token instance.</returns>
    public static UserTokenEntity Create(
        Guid userId,
        string loginProvider,
        string name,
        string value,
        TokenType type,
        DateTimeOffset? expiresAt = null,
        string metadata = "{}",
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(loginProvider))
            throw new ArgumentException("Login provider cannot be empty", nameof(loginProvider));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Token name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token value cannot be empty", nameof(value));

        var token = new UserTokenEntity(
            Guid.NewGuid(),
            userId,
            loginProvider.Trim(),
            name.Trim(),
            value.Trim(),
            type,
            expiresAt,
            metadata.Trim(),
            createdBy);

        return token;
    }

    /// <summary>
    /// Creates a new user token with a specific ID.
    /// </summary>
    /// <param name="id">The token ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="name">The token name.</param>
    /// <param name="value">The token value.</param>
    /// <param name="type">The token type.</param>
    /// <param name="expiresAt">The token expiration timestamp.</param>
    /// <param name="metadata">The token metadata.</param>
    /// <param name="createdBy">The user creating the token.</param>
    /// <returns>A new user token instance.</returns>
    public static UserTokenEntity CreateWithId(
        Guid id,
        Guid userId,
        string loginProvider,
        string name,
        string value,
        TokenType type,
        DateTimeOffset? expiresAt = null,
        string metadata = "{}",
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(loginProvider))
            throw new ArgumentException("Login provider cannot be empty", nameof(loginProvider));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Token name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token value cannot be empty", nameof(value));

        var token = new UserTokenEntity(
            id,
            userId,
            loginProvider.Trim(),
            name.Trim(),
            value.Trim(),
            type,
            expiresAt,
            metadata.Trim(),
            createdBy);

        return token;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Records token usage.
    /// </summary>
    /// <param name="usedAt">The usage timestamp.</param>
    public void RecordUsage(DateTimeOffset? usedAt = null)
    {
        if (!IsActive())
            throw new InvalidOperationException("Cannot record usage for inactive token");

        if (IsExpired())
            throw new InvalidOperationException("Cannot record usage for expired token");

        LastUsedAt = usedAt ?? DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Revokes the token.
    /// </summary>
    /// <param name="revokedBy">The user revoking the token.</param>
    public void Revoke(string revokedBy)
    {
        if (Status == TokenStatus.Revoked)
            throw new InvalidOperationException("Token is already revoked");

        Status = TokenStatus.Revoked;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = revokedBy;
    }

    /// <summary>
    /// Refreshes the token.
    /// </summary>
    /// <param name="newValue">The new token value.</param>
    /// <param name="newExpiresAt">The new expiration timestamp.</param>
    /// <param name="refreshedBy">The user refreshing the token.</param>
    public void Refresh(string newValue, DateTimeOffset? newExpiresAt, string refreshedBy)
    {
        if (!IsActive())
            throw new InvalidOperationException("Cannot refresh inactive token");

        if (string.IsNullOrWhiteSpace(newValue))
            throw new ArgumentException("Token value cannot be empty", nameof(newValue));

        Value = newValue.Trim();
        ExpiresAt = newExpiresAt;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = refreshedBy;
    }

    /// <summary>
    /// Updates the token metadata.
    /// </summary>
    /// <param name="metadata">The new metadata.</param>
    /// <param name="modifiedBy">The user modifying the metadata.</param>
    public void UpdateMetadata(string metadata, string modifiedBy)
    {
        if (!IsActive())
            throw new InvalidOperationException("Cannot update metadata for inactive token");

        Metadata = metadata.Trim();
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Checks if the token is active.
    /// </summary>
    /// <returns>True if the token is active; otherwise, false.</returns>
    public bool IsActive() => Status == TokenStatus.Active;

    /// <summary>
    /// Checks if the token is expired.
    /// </summary>
    /// <returns>True if the token is expired; otherwise, false.</returns>
    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;

    /// <summary>
    /// Checks if the token is valid for use.
    /// </summary>
    /// <returns>True if the token is valid for use; otherwise, false.</returns>
    public bool IsValid() => IsActive() && !IsExpired();

    /// <summary>
    /// Checks if the token can be modified.
    /// </summary>
    /// <returns>True if the token can be modified; otherwise, false.</returns>
    public bool CanBeModified() => IsActive();

    /// <summary>
    /// Gets the time remaining until expiration.
    /// </summary>
    /// <returns>The time remaining, or null if no expiration.</returns>
    public TimeSpan? GetTimeRemaining()
    {
        if (!ExpiresAt.HasValue)
            return null;

        var remaining = ExpiresAt.Value - DateTimeOffset.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    #endregion
} 