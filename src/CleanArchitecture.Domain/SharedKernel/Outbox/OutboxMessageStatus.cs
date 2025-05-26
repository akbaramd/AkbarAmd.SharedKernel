using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SeedWork.Outbox;

public class OutboxMessageStatus : Enumeration
{
    public static OutboxMessageStatus Pending = new(1, nameof(Pending));
    public static OutboxMessageStatus Processing = new(2, nameof(Processing));
    public static OutboxMessageStatus Processed = new(3, nameof(Processed));
    public static OutboxMessageStatus Failed = new(4, nameof(Failed));

    private OutboxMessageStatus(int id, string name) : base(id, name)
    {
    }
} 