using System;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SeedWork.Outbox
{
 

    public class OutboxMessage : EntityBase<Guid>
    {
        public string Type { get; private set; }
        public string Content { get; private set; }
        public DateTime OccurredOn { get; private set; }
        public DateTime? ProcessedOn { get; private set; }
        public string Error { get; private set; }
        public int RetryCount { get; private set; }
        public OutboxMessageStatus Status { get; private set; }

        private OutboxMessage() { } // For ORM

        public OutboxMessage(
            Guid id,
            string type,
            string content,
            DateTime occurredOn)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Type cannot be null or whitespace.", nameof(type));
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Content cannot be null or whitespace.", nameof(content));
            if (occurredOn == default)
                throw new ArgumentException("OccurredOn must be a valid date.", nameof(occurredOn));

            Type = type;
            Content = content;
            OccurredOn = occurredOn;
            Status = OutboxMessageStatus.Pending;
            RetryCount = 0;
        }

        public void MarkAsProcessed()
        {
            Status = OutboxMessageStatus.Processed;
            ProcessedOn = DateTime.UtcNow;
            Error = null;
        }

        public void MarkAsFailed(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error message cannot be null or whitespace.", nameof(error));

            Status = OutboxMessageStatus.Failed;
            Error = error;
            RetryCount++;
            ProcessedOn = null;
        }

        public void MarkAsRetrying()
        {
            if (Status != OutboxMessageStatus.Failed)
                throw new InvalidOperationException("Only failed messages can be retried.");

            Status = OutboxMessageStatus.Pending;
            Error = null;
            ProcessedOn = null;
        }
    }
}
