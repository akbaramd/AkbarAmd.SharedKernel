namespace AkbarAmd.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity is soft deleted.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntitySoftDeletedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the soft deleted entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the soft deleted entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Optional reason for the deletion.
    /// </summary>
    public string? DeletionReason { get; }

    /// <summary>
    /// Constructor for creating a new entity soft deleted event.
    /// </summary>
    /// <param name="entityId">The identity of the soft deleted entity.</param>
    /// <param name="entityType">The type name of the soft deleted entity.</param>
    /// <param name="deletedBy">Identifier of the user/system that deleted the entity.</param>
    /// <param name="deletionReason">Optional reason for the deletion.</param>
    public EntitySoftDeletedEvent(TId entityId, string entityType, string deletedBy, string? deletionReason = null)
        : base(deletedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        DeletionReason = deletionReason;
    }
}