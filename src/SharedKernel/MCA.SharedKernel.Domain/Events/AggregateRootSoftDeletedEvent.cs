namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root is soft deleted.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class AggregateRootSoftDeletedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the soft deleted aggregate root.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the soft deleted aggregate root.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The version of the aggregate root at deletion.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Optional reason for the deletion.
    /// </summary>
    public string? DeletionReason { get; }

    /// <summary>
    /// Constructor for creating a new aggregate root soft deleted event.
    /// </summary>
    /// <param name="aggregateId">The identity of the soft deleted aggregate root.</param>
    /// <param name="aggregateType">The type name of the soft deleted aggregate root.</param>
    /// <param name="version">The version of the aggregate root at deletion.</param>
    /// <param name="deletedBy">Identifier of the user/system that deleted the aggregate root.</param>
    /// <param name="deletionReason">Optional reason for the deletion.</param>
    public AggregateRootSoftDeletedEvent(TId aggregateId, string aggregateType, long version, string deletedBy, string? deletionReason = null)
        : base(deletedBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        Version = version;
        DeletionReason = deletionReason;
    }
}