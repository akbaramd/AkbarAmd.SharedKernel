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
    /// Domain event raised when an OutboxMessage processing fails.
    /// </summary>
    public class OutboxMessageFailedEvent : DomainEvent
    {
        /// <summary>
        /// Gets the ID of the failed outbox message.
        /// </summary>
        public Guid OutboxMessageId { get; }

        /// <summary>
        /// Gets the error message describing the failure.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Gets the current retry count.
        /// </summary>
        public int RetryCount { get; }

        /// <summary>
        /// Initializes a new instance of the OutboxMessageFailedEvent class.
        /// </summary>
        /// <param name="outboxMessageId">The ID of the failed outbox message.</param>
        /// <param name="error">The error message describing the failure.</param>
        /// <param name="retryCount">The current retry count.</param>
        public OutboxMessageFailedEvent(Guid outboxMessageId, string error, int retryCount)
        {
            OutboxMessageId = outboxMessageId;
            Error = error;
            RetryCount = retryCount;
        }
    }
}
