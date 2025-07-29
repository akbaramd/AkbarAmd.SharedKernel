using MCA.SharedKernel.Domain.Events;

namespace MCA.Identity.Domain.User.Events;

public class UserEmailChangedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string NewEmail { get; }

    public UserEmailChangedEvent(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
    }
}