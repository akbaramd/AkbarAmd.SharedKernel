namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity is permanently deleted.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityPermanentlyDeletedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the permanently deleted entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the permanently deleted entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Optional reason for the permanent deletion.
    /// </summary>
    public string? DeletionReason { get; }

    /// <summary>
    /// Constructor for creating a new entity permanently deleted event.
    /// </summary>
    /// <param name="entityId">The identity of the permanently deleted entity.</param>
    /// <param name="entityType">The type name of the permanently deleted entity.</param>
    /// <param name="deletedBy">Identifier of the user/system that deleted the entity.</param>
    /// <param name="deletionReason">Optional reason for the permanent deletion.</param>
    public EntityPermanentlyDeletedEvent(TId entityId, string entityType, string deletedBy, string? deletionReason = null)
        : base(deletedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        DeletionReason = deletionReason;
    }
}