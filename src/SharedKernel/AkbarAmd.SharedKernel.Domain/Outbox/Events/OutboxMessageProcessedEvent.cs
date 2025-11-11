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
    /// Domain event raised when an OutboxMessage is successfully processed.
    /// </summary>
    public class OutboxMessageProcessedEvent : DomainEvent
    {
        /// <summary>
        /// Gets the ID of the processed outbox message.
        /// </summary>
        public Guid OutboxMessageId { get; }

        /// <summary>
        /// Gets the UTC date/time when the message was processed.
        /// </summary>
        public DateTime ProcessedOn { get; }

        /// <summary>
        /// Initializes a new instance of the OutboxMessageProcessedEvent class.
        /// </summary>
        /// <param name="outboxMessageId">The ID of the processed outbox message.</param>
        /// <param name="processedOn">The UTC date/time when the message was processed.</param>
        public OutboxMessageProcessedEvent(Guid outboxMessageId, DateTime processedOn)
        {
            OutboxMessageId = outboxMessageId;
            ProcessedOn = processedOn;
        }
    }
}
