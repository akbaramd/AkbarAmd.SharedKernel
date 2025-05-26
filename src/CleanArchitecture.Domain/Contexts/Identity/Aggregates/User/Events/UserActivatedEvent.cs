using System;
using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;

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