namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity or aggregate is accessed (read).
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityAccessedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the accessed entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the accessed entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The type of access (e.g., "Read", "View", "Export").
    /// </summary>
    public string AccessType { get; }

    /// <summary>
    /// Optional context or purpose of the access.
    /// </summary>
    public string? AccessContext { get; }

    /// <summary>
    /// Constructor for creating a new entity accessed event.
    /// </summary>
    /// <param name="entityId">The identity of the accessed entity.</param>
    /// <param name="entityType">The type name of the accessed entity.</param>
    /// <param name="accessType">The type of access.</param>
    /// <param name="accessedBy">Identifier of the user/system that accessed the entity.</param>
    /// <param name="accessContext">Optional context or purpose of the access.</param>
    public EntityAccessedEvent(TId entityId, string entityType, string accessType, string accessedBy, string? accessContext = null)
        : base(accessedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        AccessType = accessType ?? throw new ArgumentNullException(nameof(accessType));
        AccessContext = accessContext;
    }
}