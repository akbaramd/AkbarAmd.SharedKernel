/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture · Identity Context
 * Entity: Role Aggregate Root
 * Year: 2025
 */

using Microsoft.AspNetCore.Identity;
using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;
using MCA.Context.Identity.Domain.User.Enumerations;

namespace MCA.Context.Identity.Domain.User.Entities;

/// <summary>
/// Represents a role in the identity system.
/// Role is an aggregate root that manages role-related business logic.
/// </summary>
public sealed class RoleEntity : IdentityRole<Guid>, IAggregateRoot<Guid>
{
    #region Properties

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public new string Name
    {
        get => base.Name ?? string.Empty;
        private set => base.Name = value;
    }

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role status.
    /// </summary>
    public RoleStatus Status { get; private set; }

    /// <summary>
    /// Gets the role type.
    /// </summary>
    public RoleType Type { get; private set; }

    /// <summary>
    /// Gets the role priority level.
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Gets whether the role is system-defined.
    /// </summary>
    public bool IsSystem { get; private set; }

    /// <summary>
    /// Gets the role permissions as a JSON string.
    /// </summary>
    public string Permissions { get; private set; } = "[]";

    /// <summary>
    /// Gets the creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the last modification timestamp.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; private set; }

    /// <summary>
    /// Gets the user who created this role.
    /// </summary>
    public string CreatedBy { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the user who last modified this role.
    /// </summary>
    public string LastModifiedBy { get; private set; } = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RoleEntity() { }

    /// <summary>
    /// Creates a new role with the specified parameters.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <param name="type">The role type.</param>
    /// <param name="priority">The role priority.</param>
    /// <param name="isSystem">Whether the role is system-defined.</param>
    /// <param name="createdBy">The user creating the role.</param>
    private RoleEntity(
        Guid id,
        string name,
        string description,
        RoleType type,
        int priority,
        bool isSystem,
        string createdBy)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Priority = priority;
        IsSystem = isSystem;
        Status = RoleStatus.Active;
        CreatedBy = createdBy;
        CreatedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = createdBy;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <param name="type">The role type.</param>
    /// <param name="priority">The role priority.</param>
    /// <param name="isSystem">Whether the role is system-defined.</param>
    /// <param name="createdBy">The user creating the role.</param>
    /// <returns>A new role instance.</returns>
    public static RoleEntity Create(
        string name,
        string description,
        RoleType type,
        int priority = 0,
        bool isSystem = false,
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description cannot be empty", nameof(description));
        if (priority < 0)
            throw new ArgumentException("Role priority cannot be negative", nameof(priority));

        var role = new RoleEntity(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim() ?? string.Empty,
            type,
            priority,
            isSystem,
            createdBy);

        return role;
    }

    /// <summary>
    /// Creates a new role with a specific ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description.</param>
    /// <param name="type">The role type.</param>
    /// <param name="priority">The role priority.</param>
    /// <param name="isSystem">Whether the role is system-defined.</param>
    /// <param name="createdBy">The user creating the role.</param>
    /// <returns>A new role instance.</returns>
    public static RoleEntity CreateWithId(
        Guid id,
        string name,
        string description,
        RoleType type,
        int priority = 0,
        bool isSystem = false,
        string createdBy = "system")
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description cannot be empty", nameof(description));
        if (priority < 0)
            throw new ArgumentException("Role priority cannot be negative", nameof(priority));

        var role = new RoleEntity(
            id,
            name.Trim(),
            description?.Trim() ?? string.Empty,
            type,
            priority,
            isSystem,
            createdBy);

        return role;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Updates the role information.
    /// </summary>
    /// <param name="name">The new role name.</param>
    /// <param name="description">The new role description.</param>
    /// <param name="type">The new role type.</param>
    /// <param name="priority">The new role priority.</param>
    /// <param name="modifiedBy">The user modifying the role.</param>
    public void Update(
        string name,
        string description,
        RoleType type,
        int priority,
        string modifiedBy)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot modify system-defined roles");

        // Validate input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Role description cannot be empty", nameof(description));
        if (priority < 0)
            throw new ArgumentException("Role priority cannot be negative", nameof(priority));

        var oldName = Name;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Type = type;
        Priority = priority;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Activates the role.
    /// </summary>
    /// <param name="activatedBy">The user activating the role.</param>
    public void Activate(string activatedBy)
    {
        if (Status == RoleStatus.Active)
            throw new InvalidOperationException("Role is already active");

        Status = RoleStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = activatedBy;
    }

    /// <summary>
    /// Deactivates the role.
    /// </summary>
    /// <param name="deactivatedBy">The user deactivating the role.</param>
    public void Deactivate(string deactivatedBy)
    {
        if (Status == RoleStatus.Inactive)
            throw new InvalidOperationException("Role is already inactive");

        if (IsSystem)
            throw new InvalidOperationException("Cannot deactivate system-defined roles");

        Status = RoleStatus.Inactive;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = deactivatedBy;
    }

    /// <summary>
    /// Deletes the role.
    /// </summary>
    /// <param name="deletedBy">The user deleting the role.</param>
    public void Delete(string deletedBy)
    {
        if (Status == RoleStatus.Deleted)
            throw new InvalidOperationException("Role is already deleted");

        if (IsSystem)
            throw new InvalidOperationException("Cannot delete system-defined roles");

        Status = RoleStatus.Deleted;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = deletedBy;
    }

    /// <summary>
    /// Updates the role permissions.
    /// </summary>
    /// <param name="permissions">The new permissions as JSON string.</param>
    /// <param name="modifiedBy">The user modifying the permissions.</param>
    public void UpdatePermissions(string permissions, string modifiedBy)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot modify system-defined role permissions");

        if (string.IsNullOrWhiteSpace(permissions))
            throw new ArgumentException("Permissions cannot be empty", nameof(permissions));

        Permissions = permissions;
        LastModifiedAt = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Checks if the role is active.
    /// </summary>
    /// <returns>True if the role is active; otherwise, false.</returns>
    public bool IsActive() => Status == RoleStatus.Active;

    /// <summary>
    /// Checks if the role is deleted.
    /// </summary>
    /// <returns>True if the role is deleted; otherwise, false.</returns>
    public bool IsDeleted() => Status == RoleStatus.Deleted;

    /// <summary>
    /// Checks if the role can be modified.
    /// </summary>
    /// <returns>True if the role can be modified; otherwise, false.</returns>
    public bool CanBeModified() => !IsSystem && Status != RoleStatus.Deleted;

    #endregion
} 