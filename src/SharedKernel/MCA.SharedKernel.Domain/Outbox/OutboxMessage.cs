/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Outbox Message Entity
 * Represents a robust domain entity for reliable outbox message handling with status tracking, retries, and auditing.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Outbox
{

    /// <summary>
    /// Represents a domain entity for an Outbox message supporting reliable event/message dispatch.
    /// Encapsulates state transitions, retries, error handling, and audit fields.
    /// </summary>
    public class OutboxMessage : Entity<Guid>
    {
        /// <summary>
        /// Gets the fully qualified .NET type name of the message content.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the serialized message content (typically JSON).
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the UTC date/time when the message was originally created or occurred.
        /// </summary>
        public DateTime OccurredOn { get; private set; }

        /// <summary>
        /// Gets the UTC date/time when the message was successfully processed.
        /// Null if not yet processed.
        /// </summary>
        public DateTime? ProcessedOn { get; private set; }

        /// <summary>
        /// Gets the last error message encountered when processing the message.
        /// Null if no error.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// Gets the number of retry attempts for processing the message.
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// Gets the current status of the outbox message.
        /// </summary>
        public OutboxMessageStatus Status { get; private set; }

        /// <summary>
        /// Gets the UTC date/time when the message entity was created.
        /// Inherited from Entity.CreatedAt or explicitly overridden if needed.
        /// </summary>
        public DateTime CreatedAtUtc => CreatedAtUtc;

        /// <summary>
        /// Gets the UTC date/time when the message entity was last updated.
        /// Tracks all state changes.
        /// </summary>
        public DateTime? LastModifiedAtUtc { get; private set; }

        // For ORM frameworks like EF Core, a private parameterless constructor is required
        private OutboxMessage() { }

        /// <summary>
        /// Creates a new OutboxMessage instance with required data.
        /// </summary>
        /// <param name="id">Unique message identifier.</param>
        /// <param name="type">Fully qualified type name of the message.</param>
        /// <param name="content">Serialized message payload.</param>
        /// <param name="occurredOn">UTC date/time the event/message occurred.</param>
        public OutboxMessage(Guid id, string type, string content, DateTime occurredOn)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be null or whitespace.", nameof(type));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));

            if (occurredOn == default)
                throw new ArgumentException("OccurredOn must be a valid non-default DateTime.", nameof(occurredOn));

            Type = type;
            Content = content;
            OccurredOn = occurredOn;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
            LastModifiedAtUtc = null;
        }

        /// <summary>
        /// Marks the message as successfully processed.
        /// </summary>
        public void MarkAsProcessed()
        {
            if (Status == OutboxMessageStatus.Processed)
                return;

            Status = OutboxMessageStatus.Processed;
            ProcessedOn = DateTime.UtcNow;
            Error = null;
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the message as failed with a provided error message.
        /// </summary>
        /// <param name="error">Error details explaining failure.</param>
        public void MarkAsFailed(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));

            if (Status == OutboxMessageStatus.Failed)
            {
                RetryCount++;
            }
            else
            {
                Status = OutboxMessageStatus.Failed;
                RetryCount = 1;
            }

            Error = error;
            ProcessedOn = null;
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the message as retrying after a failure.
        /// </summary>
        public void MarkAsRetrying()
        {
            if (Status != OutboxMessageStatus.Failed)
                throw new InvalidOperationException("Only failed messages can be marked as retrying.");

            Status = OutboxMessageStatus.Retrying;
            Error = null;
            ProcessedOn = null;
            LastModifiedAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the serialized content of the message.
        /// </summary>
        /// <param name="newContent">New serialized message payload.</param>
        public void UpdateContent(string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(newContent));

            Content = newContent;
            LastModifiedAtUtc = DateTime.UtcNow;
        }
    }
}
