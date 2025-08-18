namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an entity is modified.
/// </summary>
/// <typeparam name="TId">The type of the entity's identity.</typeparam>
public class EntityModifiedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the modified entity.
    /// </summary>
    public TId EntityId { get; }

    /// <summary>
    /// The type name of the modified entity.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Optional description of what was modified.
    /// </summary>
    public string? ModificationDescription { get; }

    /// <summary>
    /// Constructor for creating a new entity modified event.
    /// </summary>
    /// <param name="entityId">The identity of the modified entity.</param>
    /// <param name="entityType">The type name of the modified entity.</param>
    /// <param name="modifiedBy">Identifier of the user/system that modified the entity.</param>
    /// <param name="modificationDescription">Optional description of what was modified.</param>
    public EntityModifiedEvent(TId entityId, string entityType, string modifiedBy, string? modificationDescription = null)
        : base(modifiedBy)
    {
        EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        ModificationDescription = modificationDescription;
    }
}