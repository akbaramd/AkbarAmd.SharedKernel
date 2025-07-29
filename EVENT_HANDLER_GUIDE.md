# Event Handler Guide - Domain vs Integration Events

## Overview

This guide explains the differences between `EventHandler<TEvent>` and `DomainEventHandler<TEvent>`, their purposes, and when to use each one in a Clean Architecture application.

## Key Differences

### 🔄 **EventHandler<TEvent>** (Integration Events)

**Purpose**: Handles integration events for cross-bounded context communication and external system integration.

**Characteristics**:
- **Scope**: Cross-bounded context communication
- **Timing**: Published after business operations are completed
- **Audience**: External systems, other bounded contexts
- **Data**: Contains business operation results, not internal state
- **Persistence**: Usually persisted in outbox pattern for reliability

**Interface**: `IEvent`
```csharp
public interface IEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string? CorrelationId { get; }
    string Source { get; }
}
```

### 🏗️ **DomainEventHandler<TEvent>** (Domain Events)

**Purpose**: Handles domain events within the same bounded context for internal communication.

**Characteristics**:
- **Scope**: Within the same bounded context
- **Timing**: Published immediately when domain state changes
- **Audience**: Internal handlers within the same context
- **Data**: Contains domain state information
- **Persistence**: Usually not persisted, in-memory processing

**Interface**: `IDomainEvent`
```csharp
public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
```

## Usage Patterns

### 📋 **When to Use EventHandler (Integration Events)**

#### 1. Cross-Bounded Context Communication
```csharp
// UserCreatedIntegrationEvent - Published to other bounded contexts
public class UserCreatedIntegrationEvent : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string? CorrelationId { get; }
    public string Source { get; }
    
    public Guid UserId { get; }
    public string Email { get; }
    public string UserType { get; }
}

// Handler in another bounded context
public class UserCreatedIntegrationEventHandler : EventHandler<UserCreatedIntegrationEvent>
{
    public override async Task Handle(UserCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Create user profile in this bounded context
        // Send welcome email
        // Update analytics
    }
}
```

#### 2. External System Integration
```csharp
// OrderPlacedIntegrationEvent - Published to external systems
public class OrderPlacedIntegrationEvent : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string? CorrelationId { get; }
    public string Source { get; }
    
    public Guid OrderId { get; }
    public decimal TotalAmount { get; }
    public string CustomerEmail { get; }
}

// Handler for external payment system
public class PaymentSystemIntegrationHandler : EventHandler<OrderPlacedIntegrationEvent>
{
    public override async Task Handle(OrderPlacedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Send to payment gateway
        // Update external inventory system
        // Notify shipping provider
    }
}
```

#### 3. Audit and Compliance
```csharp
// UserActionAuditEvent - For compliance and audit trails
public class UserActionAuditEvent : IEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public string? CorrelationId { get; }
    public string Source { get; }
    
    public Guid UserId { get; }
    public string Action { get; }
    public string Resource { get; }
    public string IpAddress { get; }
}

// Audit handler
public class AuditEventHandler : EventHandler<UserActionAuditEvent>
{
    public override async Task Handle(UserActionAuditEvent notification, CancellationToken cancellationToken)
    {
        // Log to audit database
        // Send to compliance system
        // Generate audit reports
    }
}
```

### 🎯 **When to Use DomainEventHandler (Domain Events)**

#### 1. Internal State Synchronization
```csharp
// UserCreatedEvent - Internal domain event
public class UserCreatedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public string Email { get; }
}

// Internal handler for updating related aggregates
public class UserCreatedDomainEventHandler : DomainEventHandler<UserCreatedEvent>
{
    public override async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Update user statistics
        // Create default user preferences
        // Initialize user workspace
    }
}
```

#### 2. Business Rule Enforcement
```csharp
// UserStatusChangedEvent - Domain event for business rules
public class UserStatusChangedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid UserId { get; }
    public string OldStatus { get; }
    public string NewStatus { get; }
}

// Business rule handler
public class UserStatusChangeDomainEventHandler : DomainEventHandler<UserStatusChangedEvent>
{
    public override async Task Handle(UserStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // Check business rules
        // Update related entities
        // Trigger internal workflows
    }
}
```

#### 3. CQRS Read Model Updates
```csharp
// ProductPriceChangedEvent - For read model updates
public class ProductPriceChangedEvent : IDomainEvent
{
    public Guid Id { get; }
    public DateTime OccurredOn { get; }
    public Guid ProductId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }
}

// Read model update handler
public class ProductReadModelUpdateHandler : DomainEventHandler<ProductPriceChangedEvent>
{
    public override async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        // Update product cache
        // Refresh search index
        // Update price history
    }
}
```

## Implementation Examples

### 🔧 **EventHandler Implementation**

```csharp
public class EmailNotificationIntegrationHandler : EventHandler<UserRegisteredIntegrationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationIntegrationHandler> _logger;

    public EmailNotificationIntegrationHandler(
        IEmailService emailService,
        ILogger<EmailNotificationIntegrationHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public override async Task Handle(UserRegisteredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing user registration integration event for user {UserId}", notification.UserId);

        try
        {
            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(
                notification.Email,
                notification.FirstName,
                cancellationToken);

            // Send verification email
            await _emailService.SendVerificationEmailAsync(
                notification.Email,
                notification.VerificationToken,
                cancellationToken);

            _logger.LogInformation("Successfully sent emails for user {UserId}", notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send emails for user {UserId}", notification.UserId);
            throw;
        }
    }

    protected override Task OnEventReceived(UserRegisteredIntegrationEvent notification)
    {
        _logger.LogInformation("Received user registration integration event: {EventId}", notification.Id);
        return Task.CompletedTask;
    }

    protected override Task OnEventError(UserRegisteredIntegrationEvent notification, Exception exception)
    {
        _logger.LogError(exception, "Error processing user registration integration event: {EventId}", notification.Id);
        return Task.CompletedTask;
    }
}
```

### 🏗️ **DomainEventHandler Implementation**

```csharp
public class UserStatisticsDomainEventHandler : DomainEventHandler<UserCreatedEvent>
{
    private readonly IUserStatisticsRepository _statisticsRepository;
    private readonly ILogger<UserStatisticsDomainEventHandler> _logger;

    public UserStatisticsDomainEventHandler(
        IUserStatisticsRepository statisticsRepository,
        ILogger<UserStatisticsDomainEventHandler> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public override async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user statistics for new user {UserId}", notification.UserId);

        try
        {
            // Update daily user count
            await _statisticsRepository.IncrementDailyUserCountAsync(
                notification.OccurredOn.Date,
                cancellationToken);

            // Update total user count
            await _statisticsRepository.IncrementTotalUserCountAsync(cancellationToken);

            // Update user creation trend
            await _statisticsRepository.RecordUserCreationAsync(
                notification.UserId,
                notification.OccurredOn,
                cancellationToken);

            _logger.LogInformation("Successfully updated statistics for user {UserId}", notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update statistics for user {UserId}", notification.UserId);
            throw;
        }
    }

    protected override Task ValidateDomainEvent(UserCreatedEvent notification)
    {
        if (notification.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty");

        return Task.CompletedTask;
    }

    protected override Task OnDomainEventError(UserCreatedEvent notification, Exception exception)
    {
        _logger.LogError(exception, "Domain event processing error for user {UserId}", notification.UserId);
        return Task.CompletedTask;
    }
}
```

## Event Flow Patterns

### 🔄 **Integration Event Flow**

```
1. Domain Event Occurs (UserCreatedEvent)
   ↓
2. Domain Event Handler Processes
   ↓
3. Integration Event Published (UserCreatedIntegrationEvent)
   ↓
4. Integration Event Handler Processes
   ↓
5. External Systems Notified
```

### 🏗️ **Domain Event Flow**

```
1. Aggregate State Changes
   ↓
2. Domain Event Published (UserCreatedEvent)
   ↓
3. Domain Event Handlers Process (Multiple)
   ↓
4. Internal State Updated
```

## Best Practices

### ✅ **EventHandler Best Practices**

1. **Idempotency**: Make handlers idempotent for reliability
2. **Error Handling**: Implement proper error handling and retry logic
3. **Correlation**: Use correlation IDs for tracking across systems
4. **Persistence**: Use outbox pattern for reliable delivery
5. **Monitoring**: Add comprehensive logging and metrics

### ✅ **DomainEventHandler Best Practices**

1. **Fast Processing**: Keep handlers fast and lightweight
2. **No External Calls**: Avoid external service calls in domain handlers
3. **State Consistency**: Ensure handlers maintain domain consistency
4. **Error Propagation**: Let errors propagate to maintain consistency
5. **Minimal Dependencies**: Keep dependencies minimal

### 🚫 **Common Mistakes**

1. **Using Domain Events for Integration**: Don't use domain events for cross-context communication
2. **Using Integration Events for Internal Logic**: Don't use integration events for internal state management
3. **Mixing Event Types**: Don't mix domain and integration events in the same handler
4. **Ignoring Error Handling**: Always implement proper error handling
5. **Forgetting Idempotency**: Make integration event handlers idempotent

## Conclusion

Understanding the difference between `EventHandler` and `DomainEventHandler` is crucial for building scalable, maintainable applications:

- **Use `EventHandler`** for cross-bounded context communication and external system integration
- **Use `DomainEventHandler`** for internal state management within the same bounded context
- **Follow the patterns** and best practices for each type
- **Implement proper error handling** and monitoring for both types

This separation ensures clean architecture principles are maintained and systems remain scalable and maintainable. 