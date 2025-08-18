using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Events;

public record UserActivatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }

    public UserActivatedEvent(Guid userId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
    }
} 