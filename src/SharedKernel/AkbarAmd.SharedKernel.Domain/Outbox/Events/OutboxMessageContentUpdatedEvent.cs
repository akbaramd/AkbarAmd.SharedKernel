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
    /// Domain event raised when an OutboxMessage content is updated.
    /// </summary>
    public class OutboxMessageContentUpdatedEvent : DomainEvent
    {
        /// <summary>
        /// Gets the ID of the outbox message whose content was updated.
        /// </summary>
        public Guid OutboxMessageId { get; }

        /// <summary>
        /// Gets the new content of the message.
        /// </summary>
        public string NewContent { get; }

        /// <summary>
        /// Initializes a new instance of the OutboxMessageContentUpdatedEvent class.
        /// </summary>
        /// <param name="outboxMessageId">The ID of the outbox message whose content was updated.</param>
        /// <param name="newContent">The new content of the message.</param>
        public OutboxMessageContentUpdatedEvent(Guid outboxMessageId, string newContent)
        {
            OutboxMessageId = outboxMessageId;
            NewContent = newContent;
        }
    }
}
