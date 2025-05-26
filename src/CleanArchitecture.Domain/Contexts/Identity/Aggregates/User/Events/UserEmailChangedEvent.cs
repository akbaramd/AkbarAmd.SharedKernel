using CleanArchitecture.Domain.SharedKernel.Events;

public class UserEmailChangedEvent : DomainEventBase
{
    public Guid UserId { get; }
    public string NewEmail { get; }

    public UserEmailChangedEvent(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
    }
}
