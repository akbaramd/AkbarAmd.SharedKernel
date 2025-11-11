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
    /// Domain event raised when a new OutboxMessage is created.
    /// </summary>
    public class OutboxMessageCreatedEvent : DomainEvent
    {
        /// <summary>
        /// Gets the ID of the created outbox message.
        /// </summary>
        public Guid OutboxMessageId { get; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Gets the UTC date/time when the message occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Initializes a new instance of the OutboxMessageCreatedEvent class.
        /// </summary>
        /// <param name="outboxMessageId">The ID of the created outbox message.</param>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="occurredOn">The UTC date/time when the message occurred.</param>
        public OutboxMessageCreatedEvent(Guid outboxMessageId, string messageType, DateTime occurredOn)
        {
            OutboxMessageId = outboxMessageId;
            MessageType = messageType;
            OccurredOn = occurredOn;
        }
    }
}
