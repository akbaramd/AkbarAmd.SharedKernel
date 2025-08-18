namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when a snapshot is created for an aggregate root.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class SnapshotCreatedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the aggregate root for which the snapshot was created.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the aggregate root for which the snapshot was created.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The version of the aggregate root at the time of snapshot creation.
    /// </summary>
    public long SnapshotVersion { get; }

    /// <summary>
    /// The snapshot data.
    /// </summary>
    public object SnapshotData { get; }

    /// <summary>
    /// Constructor for creating a new snapshot created event.
    /// </summary>
    /// <param name="aggregateId">The identity of the aggregate root for which the snapshot was created.</param>
    /// <param name="aggregateType">The type name of the aggregate root for which the snapshot was created.</param>
    /// <param name="snapshotVersion">The version of the aggregate root at the time of snapshot creation.</param>
    /// <param name="snapshotData">The snapshot data.</param>
    /// <param name="createdBy">Identifier of the user/system that created the snapshot.</param>
    public SnapshotCreatedEvent(TId aggregateId, string aggregateType, long snapshotVersion, object snapshotData, string createdBy)
        : base(createdBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        SnapshotVersion = snapshotVersion;
        SnapshotData = snapshotData ?? throw new ArgumentNullException(nameof(snapshotData));
    }
}