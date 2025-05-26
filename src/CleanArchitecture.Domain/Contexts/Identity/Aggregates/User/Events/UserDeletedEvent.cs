using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;

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