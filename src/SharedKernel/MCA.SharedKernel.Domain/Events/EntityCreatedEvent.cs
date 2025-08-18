namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity is created.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityCreatedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the created entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the created entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Constructor for creating a new entity created event.
    /// </summary>
    /// <param name="entityId">The identity of the created entity.</param>
    /// <param name="entityType">The type name of the created entity.</param>
    /// <param name="createdBy">Identifier of the user/system that created the entity.</param>
    public EntityCreatedEvent(TId entityId, string entityType, string createdBy)
        : base(createdBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
    }
}