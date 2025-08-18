using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Events;

public record UserDeactivatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public string Reason { get; }

    public UserDeactivatedEvent(Guid userId, string reason = null)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        Reason = reason;
    }
} 