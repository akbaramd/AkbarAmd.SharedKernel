using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Events;

public record UserCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public string Email { get; }

    public UserCreatedEvent(Guid userId, string email)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        UserId = userId;
        Email = email;
    }
} 