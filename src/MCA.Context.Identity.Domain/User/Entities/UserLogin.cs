/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Entity: User Login
 * Year: 2025
 */

using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;
using MCA.Context.Identity.Domain.User.Enumerations;

namespace MCA.Context.Identity.Domain.User.Entities;

/// <summary>
/// Represents an external login provider associated with a user in the identity system.
/// UserLogin is an entity that belongs to the User aggregate root.
/// </summary>
public sealed class UserLoginEntity : Entity<Guid>
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
    /// Gets the provider key.
    /// </summary>
    public string ProviderKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string ProviderDisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets whether the login is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the login status.
    /// </summary>
    public LoginStatus Status { get; private set; }

    /// <summary>
    /// Gets the last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the login metadata as JSON string.
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
    /// Gets the user who created this login.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user who last modified this login.
    /// </summary>
    public string LastModifiedBy { get; private set; } = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private UserLoginEntity() { }

    /// <summary>
    /// Creates a new user login with the specified parameters.
    /// </summary>
    /// <param name="id">The login ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="providerDisplayName">The provider display name.</param>
    /// <param name="metadata">The login metadata.</param>
    /// <param name="createdBy">The user creating the login.</param>
    private UserLoginEntity(
        Guid id,
        Guid userId,
        string loginProvider,
        string providerKey,
        string providerDisplayName,
        string metadata,
        string createdBy)
        : base(id)
    {
        UserId = userId;
        LoginProvider = loginProvider;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        Metadata = metadata;
        IsActive = true;
        Status = LoginStatus.Registered;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = createdBy;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new user login.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="providerDisplayName">The provider display name.</param>
    /// <param name="metadata">The login metadata.</param>
    /// <param name="createdBy">The user creating the login.</param>
    /// <returns>A new user login instance.</returns>
    public static UserLoginEntity Create(
        Guid userId,
        string loginProvider,
        string providerKey,
        string providerDisplayName,
        string metadata = "{}",
        string createdBy = "system")
    {
        // Validate input
        CheckRule(new UserLoginProviderFormatRule(loginProvider));
        CheckRule(new UserLoginProviderKeyFormatRule(providerKey));
        CheckRule(new UserLoginProviderDisplayNameFormatRule(providerDisplayName));
        CheckRule(new UserLoginMetadataFormatRule(metadata));

        var login = new UserLoginEntity(
            Guid.NewGuid(),
            userId,
            loginProvider.Trim(),
            providerKey.Trim(),
            providerDisplayName?.Trim() ?? string.Empty,
            metadata.Trim(),
            createdBy);

        return login;
    }

    /// <summary>
    /// Creates a new user login with a specific ID.
    /// </summary>
    /// <param name="id">The login ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="providerDisplayName">The provider display name.</param>
    /// <param name="metadata">The login metadata.</param>
    /// <param name="createdBy">The user creating the login.</param>
    /// <returns>A new user login instance.</returns>
    public static UserLoginEntity CreateWithId(
        Guid id,
        Guid userId,
        string loginProvider,
        string providerKey,
        string providerDisplayName,
        string metadata = "{}",
        string createdBy = "system")
    {
        // Validate input
        CheckRule(new UserLoginProviderFormatRule(loginProvider));
        CheckRule(new UserLoginProviderKeyFormatRule(providerKey));
        CheckRule(new UserLoginProviderDisplayNameFormatRule(providerDisplayName));
        CheckRule(new UserLoginMetadataFormatRule(metadata));

        var login = new UserLoginEntity(
            id,
            userId,
            loginProvider.Trim(),
            providerKey.Trim(),
            providerDisplayName?.Trim() ?? string.Empty,
            metadata.Trim(),
            createdBy);

        return login;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Records a successful login.
    /// </summary>
    /// <param name="loginAt">The login timestamp.</param>
    public void RecordLogin(DateTimeOffset? loginAt = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot record login for inactive login provider");

        LastLoginAt = loginAt ?? DateTimeOffset.UtcNow;
        Status = LoginStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseEvent(new UserLoginRecordedEvent(Id, UserId, LoginProvider), _ => { });
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    public void RecordFailedLogin()
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot record failed login for inactive login provider");

        Status = LoginStatus.Failed;
        LastModifiedAt = DateTimeOffset.UtcNow;

        RaiseEvent(new UserLoginFailedEvent(Id, UserId, LoginProvider), _ => { });
    }

    /// <summary>
    /// Activates the login provider.
    /// </summary>
    /// <param name="activatedBy">The user activating the login.</param>
    public void Activate(string activatedBy)
    {
        if (IsActive)
            throw new InvalidOperationException("Login provider is already active");

        IsActive = true;
        Status = LoginStatus.Registered;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = activatedBy;

        RaiseEvent(new UserLoginActivatedEvent(Id, UserId, LoginProvider), _ => { });
    }

    /// <summary>
    /// Deactivates the login provider.
    /// </summary>
    /// <param name="deactivatedBy">The user deactivating the login.</param>
    public void Deactivate(string deactivatedBy)
    {
        if (!IsActive)
            throw new InvalidOperationException("Login provider is already inactive");

        IsActive = false;
        Status = LoginStatus.Disabled;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = deactivatedBy;

        RaiseEvent(new UserLoginDeactivatedEvent(Id, UserId, LoginProvider), _ => { });
    }

    /// <summary>
    /// Updates the login metadata.
    /// </summary>
    /// <param name="metadata">The new metadata.</param>
    /// <param name="modifiedBy">The user modifying the metadata.</param>
    public void UpdateMetadata(string metadata, string modifiedBy)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot update metadata for inactive login provider");

        CheckRule(new UserLoginMetadataFormatRule(metadata));

        Metadata = metadata.Trim();
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        RaiseEvent(new UserLoginMetadataUpdatedEvent(Id, UserId, LoginProvider), _ => { });
    }

    /// <summary>
    /// Checks if the login provider is active.
    /// </summary>
    /// <returns>True if the login provider is active; otherwise, false.</returns>
    public bool IsActiveLogin() => IsActive;

    /// <summary>
    /// Checks if the login provider can be used.
    /// </summary>
    /// <returns>True if the login provider can be used; otherwise, false.</returns>
    public bool CanBeUsed() => IsActive && Status != LoginStatus.Failed;

    /// <summary>
    /// Checks if the login provider can be modified.
    /// </summary>
    /// <returns>True if the login provider can be modified; otherwise, false.</returns>
    public bool CanBeModified() => IsActive;

    #endregion

    #region Domain Events

    private void RaiseEvent<TEvent>(TEvent domainEvent, Action<TEvent> stateChanger)
        where TEvent : class
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
        if (stateChanger == null) throw new ArgumentNullException(nameof(stateChanger));
        
        stateChanger(domainEvent);
        // Note: Domain event raising would be implemented in the base Entity
    }

    #endregion

    #region Business Rules

    private static void CheckRule(IBusinessRule rule)
    {
        if (rule == null) throw new ArgumentNullException(nameof(rule));
        if (!rule.IsSatisfied())
            throw new DomainBusinessRuleValidationException(rule);
    }

    #endregion
} 