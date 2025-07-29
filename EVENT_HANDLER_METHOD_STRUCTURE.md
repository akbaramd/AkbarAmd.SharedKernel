# Event Handler Method Structure - Professional Implementation

## Overview

This document explains the new method structure for `EventHandler<TEvent>` and `DomainEventHandler<TEvent>` classes, which follow professional patterns with protected implementation methods and public processor methods.

## Method Structure

### 🔧 **EventHandler<TEvent> Structure**

```csharp
public abstract class EventHandler<TEvent> : INotificationHandler<TEvent> 
    where TEvent : IEvent
{
    // Protected abstract method - Override this for business logic
    protected abstract Task ProcessAsync(TEvent notification, CancellationToken cancellationToken);

    // Public method - Called by MediatR (DO NOT OVERRIDE)
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        // Cross-cutting concerns: logging, validation, error handling
        // Calls ProcessAsync internally
    }

    // Protected virtual methods for customization
    protected virtual Task OnEventReceived(TEvent notification) { }
    protected virtual Task ValidateIntegrationEvent(TEvent notification) { }
    protected virtual Task OnEventProcessed(TEvent notification) { }
    protected virtual Task OnEventError(TEvent notification, Exception exception) { }
}
```

### 🏗️ **DomainEventHandler<TEvent> Structure**

```csharp
public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent> 
    where TEvent : class, MediatR.INotification
{
    // Protected abstract method - Override this for business logic
    protected abstract Task ProcessAsync(TEvent notification, CancellationToken cancellationToken);

    // Public method - Called by MediatR (DO NOT OVERRIDE)
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        // Cross-cutting concerns: logging, validation, error handling
        // Calls ProcessAsync internally
    }

    // Protected virtual methods for customization
    protected virtual Task OnDomainEventReceived(TEvent notification) { }
    protected virtual Task ValidateDomainEvent(TEvent notification) { }
    protected virtual Task OnDomainEventProcessed(TEvent notification) { }
    protected virtual Task OnDomainEventError(TEvent notification, Exception exception) { }
}
```

## Key Design Principles

### 🎯 **Separation of Concerns**

1. **Public Handle Method**: MediatR interface implementation with cross-cutting concerns
2. **Protected ProcessAsync Method**: Pure business logic implementation
3. **Protected Virtual Methods**: Customization hooks for logging, validation, and error handling

### 🔒 **Access Control**

- **Public**: Only the `Handle` method (MediatR requirement)
- **Protected**: All business logic and customization methods
- **Private**: Internal implementation details

### 🏗️ **Template Method Pattern**

The `Handle` method implements the template method pattern:
1. **Pre-processing**: Logging, validation
2. **Core Processing**: Call to `ProcessAsync`
3. **Post-processing**: Success logging, cleanup
4. **Error Handling**: Exception logging and re-throwing

## Implementation Examples

### 📧 **Integration Event Handler Example**

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

    // Override the protected ProcessAsync method for business logic
    protected override async Task ProcessAsync(UserRegisteredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Pure business logic - no cross-cutting concerns
        await _emailService.SendWelcomeEmailAsync(
            notification.Email,
            notification.FirstName,
            cancellationToken);

        await _emailService.SendVerificationEmailAsync(
            notification.Email,
            notification.VerificationToken,
            cancellationToken);
    }

    // Optional: Override customization methods
    protected override Task OnEventReceived(UserRegisteredIntegrationEvent notification)
    {
        _logger.LogInformation("Processing email notification for user {UserId}", notification.UserId);
        return Task.CompletedTask;
    }

    protected override Task OnEventError(UserRegisteredIntegrationEvent notification, Exception exception)
    {
        _logger.LogError(exception, "Failed to send emails for user {UserId}", notification.UserId);
        return Task.CompletedTask;
    }
}
```

### 📊 **Domain Event Handler Example**

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

    // Override the protected ProcessAsync method for business logic
    protected override async Task ProcessAsync(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Pure business logic - no cross-cutting concerns
        await _statisticsRepository.IncrementDailyUserCountAsync(
            notification.OccurredOn.Date,
            cancellationToken);

        await _statisticsRepository.IncrementTotalUserCountAsync(cancellationToken);

        await _statisticsRepository.RecordUserCreationAsync(
            notification.UserId,
            notification.OccurredOn,
            cancellationToken);
    }

    // Optional: Override validation method
    protected override Task ValidateDomainEvent(UserCreatedEvent notification)
    {
        if (notification.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty");

        return Task.CompletedTask;
    }

    // Optional: Override customization methods
    protected override Task OnDomainEventReceived(UserCreatedEvent notification)
    {
        _logger.LogInformation("Updating statistics for new user {UserId}", notification.UserId);
        return Task.CompletedTask;
    }

    protected override Task OnDomainEventProcessed(UserCreatedEvent notification)
    {
        _logger.LogInformation("Successfully updated statistics for user {UserId}", notification.UserId);
        return Task.CompletedTask;
    }
}
```

## Method Flow

### 🔄 **EventHandler Flow**

```
1. MediatR calls Handle(notification, cancellationToken)
   ↓
2. OnEventReceived(notification) - Logging
   ↓
3. ValidateIntegrationEvent(notification) - Validation
   ↓
4. ProcessAsync(notification, cancellationToken) - Business Logic
   ↓
5. OnEventProcessed(notification) - Success Logging
   ↓
6. Return to MediatR
```

### 🏗️ **DomainEventHandler Flow**

```
1. MediatR calls Handle(notification, cancellationToken)
   ↓
2. OnDomainEventReceived(notification) - Logging
   ↓
3. ValidateDomainEvent(notification) - Validation
   ↓
4. ProcessAsync(notification, cancellationToken) - Business Logic
   ↓
5. OnDomainEventProcessed(notification) - Success Logging
   ↓
6. Return to MediatR
```

### 🚨 **Error Flow**

```
1. Exception occurs in any step
   ↓
2. OnEventError(notification, exception) - Error Logging
   ↓
3. Exception is re-thrown
   ↓
4. MediatR handles the exception
```

## Best Practices

### ✅ **Do's**

1. **Override ProcessAsync**: Implement your business logic in the protected `ProcessAsync` method
2. **Use Customization Methods**: Override virtual methods for logging, validation, and error handling
3. **Keep ProcessAsync Pure**: Don't add cross-cutting concerns to `ProcessAsync`
4. **Handle Exceptions**: Let exceptions propagate through the base class
5. **Use Dependency Injection**: Inject dependencies through constructor

### ❌ **Don'ts**

1. **Don't Override Handle**: Never override the public `Handle` method
2. **Don't Add Cross-Cutting Concerns to ProcessAsync**: Keep it focused on business logic
3. **Don't Swallow Exceptions**: Let the base class handle exception logging
4. **Don't Use Static Methods**: Use instance methods for proper dependency injection
5. **Don't Mix Event Types**: Use the correct handler for your event type

## Migration Guide

### 🔄 **From Old Structure**

**Before:**
```csharp
public class OldEventHandler : EventHandler<UserEvent>
{
    public override async Task Handle(UserEvent notification, CancellationToken cancellationToken)
    {
        // Business logic mixed with cross-cutting concerns
        _logger.LogInformation("Processing event");
        
        try
        {
            await _service.DoSomethingAsync();
            _logger.LogInformation("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error");
            throw;
        }
    }
}
```

**After:**
```csharp
public class NewEventHandler : EventHandler<UserEvent>
{
    protected override async Task ProcessAsync(UserEvent notification, CancellationToken cancellationToken)
    {
        // Pure business logic
        await _service.DoSomethingAsync();
    }

    protected override Task OnEventReceived(UserEvent notification)
    {
        _logger.LogInformation("Processing event");
        return Task.CompletedTask;
    }

    protected override Task OnEventProcessed(UserEvent notification)
    {
        _logger.LogInformation("Success");
        return Task.CompletedTask;
    }

    protected override Task OnEventError(UserEvent notification, Exception exception)
    {
        _logger.LogError(exception, "Error");
        return Task.CompletedTask;
    }
}
```

## Benefits

### 🎯 **Clean Architecture**

1. **Separation of Concerns**: Business logic separated from cross-cutting concerns
2. **Single Responsibility**: Each method has a clear, focused purpose
3. **Dependency Inversion**: Depend on abstractions, not concretions

### 🔧 **Maintainability**

1. **Consistent Patterns**: All handlers follow the same structure
2. **Reusable Code**: Cross-cutting concerns implemented once in base class
3. **Easy Testing**: Business logic can be tested independently

### 🚀 **Performance**

1. **Efficient Logging**: Structured logging with proper context
2. **Error Handling**: Centralized error handling and logging
3. **Validation**: Consistent validation patterns across all handlers

## Conclusion

The new method structure provides:

- **Professional Implementation**: Follows enterprise-grade patterns
- **Clean Separation**: Business logic separated from infrastructure concerns
- **Consistent Behavior**: All handlers behave consistently
- **Easy Maintenance**: Clear patterns for developers to follow
- **Better Testing**: Business logic can be tested in isolation

This structure ensures that your event handlers are maintainable, testable, and follow clean architecture principles. 