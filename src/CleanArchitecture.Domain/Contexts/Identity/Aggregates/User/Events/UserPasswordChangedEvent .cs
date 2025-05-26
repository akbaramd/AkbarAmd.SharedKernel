using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;

public sealed class UserPasswordChangedEvent : DomainEventBase
{
    public Guid UserId { get; }

    public UserPasswordChangedEvent(Guid userId)
    {
        UserId = userId;
    }
}