using MCA.SharedKernel.Domain.Events;

namespace MCA.Context.Identity.Domain.User.Events;

public sealed class UserPasswordChangedEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserPasswordChangedEvent(Guid userId)
    {
        UserId = userId;
    }
}