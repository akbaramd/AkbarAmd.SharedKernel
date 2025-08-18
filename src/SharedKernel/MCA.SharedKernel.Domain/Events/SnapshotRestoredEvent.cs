namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root is restored from a snapshot.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class SnapshotRestoredEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the aggregate root that was restored from the snapshot.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the aggregate root that was restored from the snapshot.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The version of the snapshot that was restored.
    /// </summary>
    public long SnapshotVersion { get; }

    /// <summary>
    /// Constructor for creating a new snapshot restored event.
    /// </summary>
    /// <param name="aggregateId">The identity of the aggregate root that was restored from the snapshot.</param>
    /// <param name="aggregateType">The type name of the aggregate root that was restored from the snapshot.</param>
    /// <param name="snapshotVersion">The version of the snapshot that was restored.</param>
    /// <param name="restoredBy">Identifier of the user/system that restored the snapshot.</param>
    public SnapshotRestoredEvent(TId aggregateId, string aggregateType, long snapshotVersion, string restoredBy)
        : base(restoredBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        SnapshotVersion = snapshotVersion;
    }
}