using CleanArchitecture.Domain.SharedKernel.Events;

public sealed class UserPasswordChangedEvent : DomainEventBase
    {
        public Guid UserId { get; }

        public UserPasswordChangedEvent(Guid userId)
        {
            UserId = userId;
        }
    }