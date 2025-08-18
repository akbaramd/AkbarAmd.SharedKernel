namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root is modified.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class AggregateRootModifiedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the modified aggregate root.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the modified aggregate root.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The version of the aggregate root after modification.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Optional description of what was modified.
    /// </summary>
    public string? ModificationDescription { get; }

    /// <summary>
    /// Constructor for creating a new aggregate root modified event.
    /// </summary>
    /// <param name="aggregateId">The identity of the modified aggregate root.</param>
    /// <param name="aggregateType">The type name of the modified aggregate root.</param>
    /// <param name="version">The version of the aggregate root after modification.</param>
    /// <param name="modifiedBy">Identifier of the user/system that modified the aggregate root.</param>
    /// <param name="modificationDescription">Optional description of what was modified.</param>
    public AggregateRootModifiedEvent(TId aggregateId, string aggregateType, long version, string modifiedBy, string? modificationDescription = null)
        : base(modifiedBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        Version = version;
        ModificationDescription = modificationDescription;
    }
}