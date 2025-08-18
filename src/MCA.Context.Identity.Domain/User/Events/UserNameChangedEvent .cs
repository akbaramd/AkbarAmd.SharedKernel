using MCA.SharedKernel.Domain.Events;

namespace MCA.Context.Identity.Domain.User.Events;

 public sealed class UserNameChangedEvent : DomainEvent
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