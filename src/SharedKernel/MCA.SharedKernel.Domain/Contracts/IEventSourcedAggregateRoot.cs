namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support event sourcing capabilities.
/// </summary>
public interface IEventSourcedAggregateRoot
{
    /// <summary>
    /// Gets the sequence number of the last applied event.
    /// </summary>
    long LastEventSequence { get; }

    /// <summary>
    /// Applies a domain event to the aggregate.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply.</param>
    void ApplyEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Gets all domain events that have been applied to this aggregate.
    /// </summary>
    /// <returns>Collection of applied domain events.</returns>
    IEnumerable<IDomainEvent> GetUncommittedEvents();

    /// <summary>
    /// Marks all events as committed.
    /// </summary>
    void MarkEventsAsCommitted();
}