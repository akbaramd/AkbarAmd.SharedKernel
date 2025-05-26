using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SeedWork.Outbox;

public class OutboxMessageStatus : Enumeration
{
    public static OutboxMessageStatus Pending = new(1, nameof(Pending), "The message is pending to be processed.");
    public static OutboxMessageStatus Processing = new(2, nameof(Processing), "The message is being processed.");
    public static OutboxMessageStatus Processed = new(3, nameof(Processed), "The message has been processed successfully.");
    public static OutboxMessageStatus Failed = new(4, nameof(Failed), "The message processing failed.");

    private OutboxMessageStatus(int id, string name, string? description = null) : base(id, name, description)
    {
    }
} 