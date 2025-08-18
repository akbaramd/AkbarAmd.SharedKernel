namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Domain event raised when an aggregate root is created.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identity.</typeparam>
public class AggregateRootCreatedEvent<TId> : AuditableDomainEvent
{
    /// <summary>
    /// The identity of the created aggregate root.
    /// </summary>
    public TId AggregateId { get; }

    /// <summary>
    /// The type name of the created aggregate root.
    /// </summary>
    public string AggregateType { get; }

    /// <summary>
    /// Constructor for creating a new aggregate root created event.
    /// </summary>
    /// <param name="aggregateId">The identity of the created aggregate root.</param>
    /// <param name="aggregateType">The type name of the created aggregate root.</param>
    /// <param name="createdBy">Identifier of the user/system that created the aggregate root.</param>
    public AggregateRootCreatedEvent(TId aggregateId, string aggregateType, string createdBy)
        : base(createdBy)
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AggregateType = aggregateType ?? throw new ArgumentNullException(nameof(aggregateType));
    }
}