namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity or aggregate is archived.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityArchivedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the archived entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the archived entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The archive location or identifier.
    /// </summary>
    public string ArchiveLocation { get; }

    /// <summary>
    /// Optional reason for archiving.
    /// </summary>
    public string? ArchiveReason { get; }

    /// <summary>
    /// Constructor for creating a new entity archived event.
    /// </summary>
    /// <param name="entityId">The identity of the archived entity.</param>
    /// <param name="entityType">The type name of the archived entity.</param>
    /// <param name="archiveLocation">The archive location or identifier.</param>
    /// <param name="archivedBy">Identifier of the user/system that performed the archiving.</param>
    /// <param name="archiveReason">Optional reason for archiving.</param>
    public EntityArchivedEvent(TId entityId, string entityType, string archiveLocation, string archivedBy, string? archiveReason = null)
        : base(archivedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        ArchiveLocation = archiveLocation ?? throw new ArgumentNullException(nameof(archiveLocation));
        ArchiveReason = archiveReason;
    }
}