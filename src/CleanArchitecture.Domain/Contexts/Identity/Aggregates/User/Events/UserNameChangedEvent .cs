using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Events;

 public sealed class UserNameChangedEvent : DomainEventBase
    {
        public Guid UserId { get; }
        public string NewFirstName { get; }
        public string NewLastName { get; }

        public UserNameChangedEvent(Guid userId, string newFirstName, string newLastName)
        {
            UserId = userId;
            NewFirstName = newFirstName ?? throw new ArgumentNullException(nameof(newFirstName));
            NewLastName = newLastName ?? throw new ArgumentNullException(nameof(newLastName));
        }
    }