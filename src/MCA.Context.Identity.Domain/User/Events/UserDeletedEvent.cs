using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Events;

public record UserDeletedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }

    public UserDeletedEvent(Guid userId)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
    }
} 