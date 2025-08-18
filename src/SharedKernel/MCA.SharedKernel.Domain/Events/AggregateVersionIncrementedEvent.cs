namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root's version is incremented.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class AggregateVersionIncrementedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the aggregate root whose version was incremented.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the aggregate root whose version was incremented.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// The previous version number.
    /// </summary>
    public long PreviousVersion { get; }

    /// <summary>
    /// The new version number.
    /// </summary>
    public long NewVersion { get; }

    /// <summary>
    /// Constructor for creating a new aggregate version incremented event.
    /// </summary>
    /// <param name="aggregateId">The identity of the aggregate root whose version was incremented.</param>
    /// <param name="aggregateType">The type name of the aggregate root whose version was incremented.</param>
    /// <param name="previousVersion">The previous version number.</param>
    /// <param name="newVersion">The new version number.</param>
    /// <param name="incrementedBy">Identifier of the user/system that caused the version increment.</param>
    public AggregateVersionIncrementedEvent(TId aggregateId, string aggregateType, long previousVersion, long newVersion, string incrementedBy)
        : base(incrementedBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
        PreviousVersion = previousVersion;
        NewVersion = newVersion;
    }
}