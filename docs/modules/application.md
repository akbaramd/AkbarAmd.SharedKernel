# AkbarAmd.SharedKernel.Application

The Application layer orchestrates business operations and coordinates between the Domain and Infrastructure layers. This module provides CQRS patterns, command/query handlers, DTOs, and service result models for building maintainable application services.

## Overview

This module defines:
- **CQRS contracts** (Commands, Queries, Events)
- **Handler base classes** for MediatR integration
- **DTOs** for data transfer between layers
- **Service results** for standardized operation outcomes
- **Mapping utilities** for object transformation

## Key Components

### CQRS Contracts

- `ICommand`: Base interface for commands (state-changing operations)
- `ICommand<TResult>`: Command that returns a result
- `IQuery<TResult>`: Base interface for queries (read operations)
- `IEvent`: Integration event interface for cross-bounded context communication

### Handler Base Classes

- `CommandHandler<TCommand>`: Base class for command handlers
- `CommandHandler<TCommand, TResult>`: Base class for command handlers with return values
- `QueryHandler<TQuery, TResult>`: Base class for query handlers
- `EventHandler<TEvent>`: Base class for event handlers
- `DomainEventHandler<TEvent>`: Base class for domain event handlers

### Service Results

- `ServiceResult`: Result model for operations without return data
- `ServiceResult<T>`: Result model for operations with return data
- `ServiceBase`: Base class for service implementations with helper methods
- `ServiceError`: Detailed error information with categories and targets
- `ServiceResultPaged<T>`: Paginated query result model

For detailed information, see:
- [ServiceResult Pattern](concepts/application/service-result.md)
- [ServiceBase](concepts/application/service-base.md)

### DTOs

- `EntityDto<TKey>`: Base DTO for entities
- `AggregateDto<TKey>`: Base DTO for aggregate roots
- `CreatableAggregateDto<TKey>`: DTO with creation tracking
- `ModifiableAggregateDto<TKey>`: DTO with modification tracking
- `DeletableAggregateDto<TKey>`: DTO with deletion tracking

## Features

- **CQRS Pattern**: Clean separation of commands and queries
- **MediatR Integration**: Decoupled communication via mediator pattern
- **Handler Lifecycle**: Built-in validation, logging, and error handling hooks
- **Service Results**: Standardized operation outcomes with error handling
- **DTO Support**: Base DTOs for common aggregate root patterns
- **Mapping Abstraction**: Framework-agnostic mapping interface

## Installation

```bash
dotnet add package AkbarAmd.SharedKernel.Application
```

## Usage Example

### Creating a Command Handler

```csharp
public class CreateUserCommand : ICommand<Guid>
{
    public string Email { get; set; }
    public string Name { get; set; }
}

public class CreateUserCommandHandler : CommandHandler<CreateUserCommand, Guid>
{
    private readonly IRepository<User, Guid> _userRepository;

    public CreateUserCommandHandler(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    protected override async Task<Guid> ProcessAsync(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.Name);
        await _userRepository.AddAsync(user, cancellationToken);
        return user.Id;
    }
}
```

## Related Concepts

- [ServiceResult](concepts/application/service-result.md) - Learn about the service result pattern
- [ServiceBase](concepts/application/service-base.md) - Understand service base class usage
- [CQRS](concepts/cqrs/cqrs.md) - Learn about Command Query Responsibility Segregation
- [Repository](concepts/ddd/repository.md) - Understand repository pattern usage
- [Domain Events](concepts/ddd/domain-events.md) - See how domain events are handled

## Dependencies

- **MediatR** 12.5.0: Mediator pattern implementation
- **FluentValidation** 11.5.0: Validation framework
- **AutoMapper** 12.0.1: Object mapping
- **Microsoft.Extensions.Logging.Abstractions** 8.0.3: Logging abstractions
- **Bonyan** 1.5.6: Modularity framework

