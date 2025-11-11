namespace AkbarAmd.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity is restored from soft deletion.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityRestoredEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the restored entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the restored entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Optional reason for the restoration.
    /// </summary>
    public string? RestorationReason { get; }

    /// <summary>
    /// Constructor for creating a new entity restored event.
    /// </summary>
    /// <param name="entityId">The identity of the restored entity.</param>
    /// <param name="entityType">The type name of the restored entity.</param>
    /// <param name="restoredBy">Identifier of the user/system that restored the entity.</param>
    /// <param name="restorationReason">Optional reason for the restoration.</param>
    public EntityRestoredEvent(TId entityId, string entityType, string restoredBy, string? restorationReason = null)
        : base(restoredBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        RestorationReason = restorationReason;
    }
}