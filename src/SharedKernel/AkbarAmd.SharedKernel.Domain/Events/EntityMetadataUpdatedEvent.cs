namespace AkbarAmd.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity's metadata is updated.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityMetadataUpdatedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the entity whose metadata was updated.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the entity whose metadata was updated.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The previous metadata value.
    /// </summary>
    public string? PreviousMetadata { get; }

    /// <summary>
    /// The new metadata value.
    /// </summary>
    public string? NewMetadata { get; }

    /// <summary>
    /// Constructor for creating a new entity metadata updated event.
    /// </summary>
    /// <param name="entityId">The identity of the entity whose metadata was updated.</param>
    /// <param name="entityType">The type name of the entity whose metadata was updated.</param>
    /// <param name="previousMetadata">The previous metadata value.</param>
    /// <param name="newMetadata">The new metadata value.</param>
    /// <param name="updatedBy">Identifier of the user/system that updated the metadata.</param>
    public EntityMetadataUpdatedEvent(TId entityId, string entityType, string? previousMetadata, string? newMetadata, string updatedBy)
        : base(updatedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        PreviousMetadata = previousMetadata;
        NewMetadata = newMetadata;
    }
}