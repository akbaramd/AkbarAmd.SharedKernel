namespace MCA.SharedKernel.Domain.Events;

/// <summary>
/// Base class for auditable domain events that include user context.
/// All auditable events should inherit from this base class.
/// </summary>
public abstract class AuditableDomainEvent : DomainEvent
{
    /// <summary>
    /// Identifier of the user/system that triggered the event.
    /// </summary>
    public string TriggeredBy { get; }

    /// <summary>
    /// UTC timestamp when the event occurred (UTC).
    /// </summary>
    public DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// Constructor for creating a new auditable domain event.
    /// </summary>
    /// <param name="triggeredBy">Identifier of the user/system that triggered the event.</param>
    protected AuditableDomainEvent(string triggeredBy)
    {
        TriggeredBy = triggeredBy ?? throw new ArgumentNullException(nameof(triggeredBy));
        OccurredOnUtc = DateTimeOffset.UtcNow;
    }
}