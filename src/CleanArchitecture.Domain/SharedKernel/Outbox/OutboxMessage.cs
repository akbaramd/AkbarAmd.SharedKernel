using System;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SeedWork.Outbox;

public class OutboxMessage : EntityBase
{
    public string Type { get; private set; }
    public string Content { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; private set; }
    public string Error { get; private set; }
    public int RetryCount { get; private set; }
    public OutboxMessageStatus Status { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(
        Guid id,
        string type,
        string content,
        DateTime occurredOn)
        : base(id)
    {
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
    }

    public void MarkAsFailed(string error)
    {
        Status = OutboxMessageStatus.Failed;
        Error = error;
        RetryCount++;
    }

    public void MarkAsRetrying()
    {
        Status = OutboxMessageStatus.Pending;
        Error = null;
    }
} 