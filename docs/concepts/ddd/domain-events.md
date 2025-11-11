# Domain Events

Domain Events represent something important that happened in the domain. They are used to communicate state changes between aggregates and trigger side effects in a decoupled way.

## Installation

To use Domain Events, install the following modules:

```bash
# Core domain module (required for domain event interfaces and aggregate root support)
dotnet add package AkbarAmd.SharedKernel.Domain

# Application module (required for domain event handlers)
dotnet add package AkbarAmd.SharedKernel.Application
```

## Overview

Domain Events:
- Represent significant business occurrences
- Are raised by aggregate roots
- Are published after the transaction commits
- Enable loose coupling between aggregates
- Support eventual consistency

## IDomainEvent Interface

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
```

## Raising Domain Events

Domain events are raised within aggregate roots:

```csharp
public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }

    public void ChangeEmail(string newEmail)
    {
        var oldEmail = Email;
        Email = newEmail;
        
        // Raise domain event
        RaiseDomainEvent(new UserEmailChangedEvent(Id, oldEmail, newEmail));
    }
}
```

## Creating Domain Events

```csharp
public class UserEmailChangedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid UserId { get; }
    public string OldEmail { get; }
    public string NewEmail { get; }

    public UserEmailChangedEvent(Guid userId, string oldEmail, string newEmail)
    {
        UserId = userId;
        OldEmail = oldEmail;
        NewEmail = newEmail;
    }
}
```

## Built-in Domain Events

The domain module provides several built-in domain events:

### Aggregate Root Events

- `AggregateRootCreatedEvent<TKey>`: Raised when an aggregate is created
- `AggregateRootModifiedEvent<TKey>`: Raised when an aggregate is modified
- `AggregateRootSoftDeletedEvent<TKey>`: Raised when an aggregate is soft deleted
- `AggregateRootRestoredEvent<TKey>`: Raised when an aggregate is restored
- `AggregateVersionIncrementedEvent<TKey>`: Raised when version is incremented

### Entity Events

- `EntityCreatedEvent<TKey>`: Raised when an entity is created
- `EntityModifiedEvent<TKey>`: Raised when an entity is modified
- `EntitySoftDeletedEvent<TKey>`: Raised when an entity is soft deleted
- `EntityRestoredEvent<TKey>`: Raised when an entity is restored
- `EntityPermanentlyDeletedEvent<TKey>`: Raised when an entity is permanently deleted

## Handling Domain Events

### In Application Layer

```csharp
public class UserEmailChangedEventHandler : DomainEventHandler<UserEmailChangedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IAuditLogRepository _auditLogRepository;

    public UserEmailChangedEventHandler(
        IEmailService emailService,
        IAuditLogRepository auditLogRepository)
    {
        _emailService = emailService;
        _auditLogRepository = auditLogRepository;
    }

    protected override async Task HandleAsync(
        UserEmailChangedEvent domainEvent, 
        CancellationToken cancellationToken)
    {
        // Send notification email
        await _emailService.SendEmailChangedNotificationAsync(
            domainEvent.UserId, 
            domainEvent.NewEmail, 
            cancellationToken);

        // Log audit trail
        await _auditLogRepository.AddAsync(new AuditLog
        {
            EntityId = domainEvent.UserId,
            EventType = "UserEmailChanged",
            OldValue = domainEvent.OldEmail,
            NewValue = domainEvent.NewEmail,
            OccurredOn = domainEvent.OccurredOn
        }, cancellationToken: cancellationToken);
    }
}
```

### Publishing Domain Events

```csharp
public class ChangeEmailCommandHandler : CommandHandler<ChangeEmailCommand>
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IMediator _mediator;

    protected override async Task ProcessAsync(
        ChangeEmailCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        user.ChangeEmail(request.NewEmail);
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
        
        // Publish domain events after save
        await PublishDomainEventsAsync(user, cancellationToken);
    }

    private async Task PublishDomainEventsAsync(
        User user, 
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in user.DomainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
        user.ClearDomainEvents();
    }
}
```

## Outbox Pattern

For reliable event publishing in distributed systems, use the Outbox pattern:

```csharp
public class OutboxMessage : AggregateRoot<Guid>
{
    public string EventType { get; private set; }
    public string EventData { get; private set; }
    public OutboxMessageStatus Status { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(IDomainEvent domainEvent)
    {
        Id = Guid.NewGuid();
        EventType = domainEvent.GetType().Name;
        EventData = JsonSerializer.Serialize(domainEvent);
        Status = OutboxMessageStatus.Pending;
    }

    public void MarkAsProcessed()
    {
        Status = OutboxMessageStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        RaiseDomainEvent(new OutboxMessageProcessedEvent(Id));
    }

    public void MarkAsFailed()
    {
        RetryCount++;
        if (RetryCount >= 3)
        {
            Status = OutboxMessageStatus.Failed;
        }
        else
        {
            Status = OutboxMessageStatus.Pending;
        }
    }
}
```

## Best Practices

1. **Keep events focused**: Each event should represent a single business occurrence
2. **Make events immutable**: Domain events should be immutable once created
3. **Include necessary data**: Include all data needed by event handlers
4. **Use past tense**: Name events in past tense (e.g., `UserEmailChanged`, not `ChangeUserEmail`)
5. **Handle failures gracefully**: Implement retry logic for critical event handlers
6. **Use Outbox pattern**: For distributed systems, use the Outbox pattern for reliable delivery

## Benefits

- **Loose Coupling**: Aggregates don't need direct references to each other
- **Eventual Consistency**: Supports eventual consistency between aggregates
- **Audit Trail**: Provides a complete history of domain changes
- **Integration**: Enables integration with external systems
- **Testability**: Easy to test event-driven workflows

## Related Topics

- [Aggregate Root](aggregate-root.md) - Learn how to raise domain events
- [CQRS](../cqrs/cqrs.md) - See how domain events fit into CQRS architecture
- [DDD Overview](index.md) - Learn about DDD concepts
- [Domain Module](../../modules/domain.md) - Explore built-in domain events

