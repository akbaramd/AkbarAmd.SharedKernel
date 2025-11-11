/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Outbox Message Events
 * Domain events for OutboxMessage aggregate root state changes.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Events;

namespace AkbarAmd.SharedKernel.Domain.Outbox.Events
{
    /// <summary>
    /// Domain event raised when an OutboxMessage is marked for retry.
    /// </summary>
    public class OutboxMessageRetryingEvent : DomainEvent
    {
        /// <summary>
        /// Gets the ID of the outbox message being retried.
        /// </summary>
        public Guid OutboxMessageId { get; }

        /// <summary>
        /// Gets the current retry count.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Initializes a new instance of the OutboxMessageRetryingEvent class.
        /// </summary>
        /// <param name="outboxMessageId">The ID of the outbox message being retried.</param>
        /// <param name="retryCount">The current retry count.</param>
        public OutboxMessageRetryingEvent(Guid outboxMessageId, int retryCount)
        {
            OutboxMessageId = outboxMessageId;
            RetryCount = retryCount;
        }
    }
}
