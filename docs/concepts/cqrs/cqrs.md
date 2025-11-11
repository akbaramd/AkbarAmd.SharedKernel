# CQRS (Command Query Responsibility Segregation)

CQRS is a pattern that separates read and write operations. Commands change state, while queries return data without side effects.

## Installation

To use CQRS, install the following modules:

```bash
# Application module (required for CQRS contracts and handler base classes)
dotnet add package AkbarAmd.SharedKernel.Application

# Domain module (required if working with domain entities in commands/queries)
dotnet add package AkbarAmd.SharedKernel.Domain

# EF Core infrastructure (optional, if using repositories in handlers)
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Overview

CQRS provides:
- **Clear separation** between commands and queries
- **Different models** for reading and writing
- **Scalability** - can scale reads and writes independently
- **Optimization** - optimize read and write paths separately
- **Simplified models** - no need to support both operations in one model

## Key Interfaces

### ICommand

Represents a command (write operation):

```csharp
public interface ICommand : IRequest<ServiceResult>
{
}

public interface ICommand<TResult> : IRequest<ServiceResult<TResult>>
{
}
```

### IQuery

Represents a query (read operation):

```csharp
public interface IQuery<TResult> : IRequest<ServiceResult<TResult>>
{
}
```

### IEvent

Represents an integration event:

```csharp
public interface IEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
    string? CorrelationId { get; set; }
    string Source { get; }
}
```

## Command Handlers

Commands change state and return a result:

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
        await _userRepository.AddAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
        return user.Id;
    }
}
```

### Command Without Return Value

```csharp
public class UpdateUserCommand : ICommand
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
}

public class UpdateUserCommandHandler : CommandHandler<UpdateUserCommand>
{
    private readonly IRepository<User, Guid> _userRepository;

    protected override async Task ProcessAsync(
        UpdateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException($"User with id {request.UserId} not found");
            
        user.UpdateName(request.Name);
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
    }
}
```

## Query Handlers

Queries return data without side effects:

```csharp
public class GetUserQuery : IQuery<UserDto>
{
    public Guid UserId { get; set; }
}

public class GetUserQueryHandler : QueryHandler<GetUserQuery, UserDto>
{
    private readonly IReadOnlyRepository<User, Guid> _userRepository;
    private readonly IMapper _mapper;

    public GetUserQueryHandler(
        IReadOnlyRepository<User, Guid> userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    protected override async Task<UserDto> ProcessAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException($"User with id {request.UserId} not found");
            
        return _mapper.Map<UserDto>(user);
    }
}
```

### Query with Pagination

```csharp
public class GetUsersQuery : IQuery<PaginatedResult<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool? IsActive { get; set; }
}

public class GetUsersQueryHandler : QueryHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IReadOnlyRepository<User, Guid> _userRepository;
    private readonly IMapper _mapper;

    protected override async Task<PaginatedResult<UserDto>> ProcessAsync(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var spec = new CriteriaBuilder<User>();
        
        if (request.IsActive.HasValue)
        {
            spec.Where(u => u.IsActive == request.IsActive.Value);
        }
        
        spec.OrderBy(u => u.CreatedAt);
        var specification = spec.Build();

        var result = await _userRepository.GetPaginatedAsync(
            request.PageNumber,
            request.PageSize,
            specification,
            cancellationToken);

        var userDtos = _mapper.Map<IEnumerable<UserDto>>(result.Items);
        
        return new PaginatedResult<UserDto>(
            userDtos,
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }
}
```

## Event Handlers

Handle integration events:

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

public class UserCreatedEventHandler : EventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    protected override async Task HandleAsync(
        UserCreatedEvent @event, 
        CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Email, cancellationToken);
    }
}
```

## Using CQRS

### Sending Commands

```csharp
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
```

### Sending Queries

```csharp
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

## Service Results

All handlers return `ServiceResult` or `ServiceResult<T>`:

```csharp
public class GetUserQueryHandler : QueryHandler<GetUserQuery, UserDto>
{
    protected override async Task<UserDto> ProcessAsync(
        GetUserQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            // Handler base class will wrap this in a failed ServiceResult
            throw new NotFoundException("User not found");
        }
        
        return _mapper.Map<UserDto>(user);
    }
}
```

## Benefits

- **Separation of Concerns**: Clear separation between reads and writes
- **Scalability**: Scale read and write models independently
- **Optimization**: Optimize each path separately
- **Simplicity**: Simpler models focused on single responsibility
- **Flexibility**: Can use different data stores for reads and writes
- **Testability**: Easy to test commands and queries independently

## Best Practices

1. **Use commands for state changes**: All modifications should go through commands
2. **Use queries for reads**: All reads should go through queries
3. **Keep handlers focused**: Each handler should do one thing
4. **Return ServiceResult**: Use ServiceResult for consistent error handling
5. **Validate in handlers**: Perform validation in command handlers
6. **Use specifications**: Use specifications for complex queries

## Related Topics

- [Repository](../ddd/repository.md) - Use repositories in handlers
- [Specifications](../ddd/specifications.md) - Build complex queries with specifications
- [Domain Events](../ddd/domain-events.md) - Raise domain events from commands
- [CQRS Overview](index.md) - Learn about CQRS concepts
- [Application Module](../../modules/application.md) - See CQRS contracts and handlers

