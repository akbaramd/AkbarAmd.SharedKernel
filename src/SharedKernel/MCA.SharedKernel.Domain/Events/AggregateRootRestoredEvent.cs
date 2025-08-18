namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root is restored from soft deletion.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class AggregateRootRestoredEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the restored aggregate root.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the restored aggregate root.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The version of the aggregate root after restoration.
    /// </summary>
    public long Version { get; }

    /// <summary>
    /// Optional reason for the restoration.
    /// </summary>
    public string? RestorationReason { get; }

    /// <summary>
    /// Constructor for creating a new aggregate root restored event.
    /// </summary>
    /// <param name="aggregateId">The identity of the restored aggregate root.</param>
    /// <param name="aggregateType">The type name of the restored aggregate root.</param>
    /// <param name="version">The version of the aggregate root after restoration.</param>
    /// <param name="restoredBy">Identifier of the user/system that restored the aggregate root.</param>
    /// <param name="restorationReason">Optional reason for the restoration.</param>
    public AggregateRootRestoredEvent(TId aggregateId, string aggregateType, long version, string restoredBy, string? restorationReason = null)
        : base(restoredBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        Version = version;
        RestorationReason = restorationReason;
    }
}