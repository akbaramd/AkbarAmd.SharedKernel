namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity or aggregate is backed up.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityBackedUpEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the backed up entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the backed up entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The backup location or identifier.
    /// </summary>
    public string BackupLocation { get; }

    /// <summary>
    /// Constructor for creating a new entity backed up event.
    /// </summary>
    /// <param name="entityId">The identity of the backed up entity.</param>
    /// <param name="entityType">The type name of the backed up entity.</param>
    /// <param name="backupLocation">The backup location or identifier.</param>
    /// <param name="backedUpBy">Identifier of the user/system that performed the backup.</param>
    public EntityBackedUpEvent(TId entityId, string entityType, string backupLocation, string backedUpBy)
        : base(backedUpBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        BackupLocation = backupLocation ?? throw new ArgumentNullException(nameof(backupLocation));
    }
}