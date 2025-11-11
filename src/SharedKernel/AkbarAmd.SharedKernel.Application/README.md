# AkbarAmd.SharedKernel.Application

Clean Architecture Application Layer - CQRS, MediatR, Validation, and Application Services

## Overview

The Application layer orchestrates business operations and coordinates between the Domain and Infrastructure layers. This package provides CQRS patterns, command/query handlers, DTOs, and service result models for building maintainable application services.

## Purpose

This layer defines:
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
- `ValidationResult`: Validation error result model
- `PaginatedResult<T>`: Paginated query result model

### DTOs
- `EntityDto<TKey>`: Base DTO for entities
- `AggregateDto<TKey>`: Base DTO for aggregate roots
- `CreatableAggregateDto<TKey>`: DTO with creation tracking
- `ModifiableAggregateDto<TKey>`: DTO with modification tracking
- `DeletableAggregateDto<TKey>`: DTO with deletion tracking

### Mapping
- `IMapper`: Mapping interface abstraction
- `ServiceCollectionMapperExtensions`: Extension methods for mapper registration

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

## Usage Examples

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

### Creating a Query Handler

```csharp
public class GetUserQuery : IQuery<UserDto>
{
    public Guid UserId { get; set; }
}

public class GetUserQueryHandler : QueryHandler<GetUserQuery, UserDto>
{
    private readonly IReadOnlyRepository<User, Guid> _userRepository;
    private readonly IMapper _mapper;

    protected override async Task<UserDto> ProcessAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }
}
```

### Using Service Results

```csharp
public async Task<ServiceResult<UserDto>> GetUserAsync(Guid userId)
{
    var user = await _userRepository.GetByIdAsync(userId);
    
    if (user == null)
        return ServiceResult<UserDto>.Failure("User not found", "USER_NOT_FOUND");
    
    var dto = _mapper.Map<UserDto>(user);
    return ServiceResult<UserDto>.Success(dto);
}
```

### Creating Integration Events

```csharp
public class UserCreatedEvent : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public string Source { get; } = "UserService";
    
    public Guid UserId { get; }
    public string Email { get; }

    public UserCreatedEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
```

## Dependencies

- **MediatR** 12.5.0: Mediator pattern implementation
- **FluentValidation** 11.5.0: Validation framework
- **AutoMapper** 12.0.1: Object mapping
- **Microsoft.Extensions.Logging.Abstractions** 8.0.3: Logging abstractions
- **Bonyan** 1.5.6: Modularity framework

## Architecture Principles

This layer follows Clean Architecture principles:
- **Depends on Domain layer** only
- **No infrastructure dependencies** (except abstractions)
- **Application logic** orchestration
- **Framework integration** (MediatR, AutoMapper)

## License

MIT License - see LICENSE file for details.

## Author

Akbar Ahmadi Saray - [GitHub](https://github.com/akbarahmadi)
