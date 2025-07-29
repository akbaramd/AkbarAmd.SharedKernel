# Command and Query Handler Guide - DDD Professional Implementation

## Overview

This guide explains the enhanced `CommandHandler<TCommand>` and `QueryHandler<TQuery, TResult>` classes that follow Domain-Driven Design (DDD) principles and CQRS patterns. These handlers provide professional, enterprise-grade implementations with cross-cutting concerns and proper separation of responsibilities.

## Command Handler Structure

### 🔧 **CommandHandler<TCommand> (Void Commands)**

```csharp
public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand> 
    where TCommand : IRequest
{
    // Protected abstract method - Override for business logic
    protected abstract Task ProcessAsync(TCommand request, CancellationToken cancellationToken);

    // Public method - Called by MediatR (DO NOT OVERRIDE)
    public async Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        // Cross-cutting concerns: logging, validation, error handling
        // Calls ProcessAsync internally
    }

    // Protected virtual methods for customization
    protected virtual Task OnCommandReceived(TCommand request) { }
    protected virtual Task ValidateCommand(TCommand request) { }
    protected virtual Task OnCommandProcessed(TCommand request) { }
    protected virtual Task OnCommandError(TCommand request, Exception exception) { }
}
```

### 🎯 **CommandHandler<TCommand, TResult> (Commands with Results)**

```csharp
public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult> 
    where TCommand : IRequest<TResult>
{
    // Protected abstract method - Override for business logic
    protected abstract Task<TResult> ProcessAsync(TCommand request, CancellationToken cancellationToken);

    // Public method - Called by MediatR (DO NOT OVERRIDE)
    public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        // Cross-cutting concerns: logging, validation, error handling
        // Calls ProcessAsync internally and returns result
    }

    // Protected virtual methods for customization
    protected virtual Task OnCommandReceived(TCommand request) { }
    protected virtual Task ValidateCommand(TCommand request) { }
    protected virtual Task OnCommandProcessed(TCommand request, TResult result) { }
    protected virtual Task OnCommandError(TCommand request, Exception exception) { }
}
```

## Query Handler Structure

### 📊 **QueryHandler<TQuery, TResult>**

```csharp
public abstract class QueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult> 
    where TQuery : IQuery<TResult>
{
    // Protected abstract method - Override for business logic
    protected abstract Task<TResult> ProcessAsync(TQuery request, CancellationToken cancellationToken);

    // Public method - Called by MediatR (DO NOT OVERRIDE)
    public async Task<TResult> Handle(TQuery request, CancellationToken cancellationToken)
    {
        // Cross-cutting concerns: logging, validation, caching, error handling
        // Calls ProcessAsync internally and returns result
    }

    // Protected virtual methods for customization
    protected virtual Task OnQueryReceived(TQuery request) { }
    protected virtual Task ValidateQuery(TQuery request) { }
    protected virtual Task<TResult?> TryGetFromCache(TQuery request) { }
    protected virtual Task CacheResult(TQuery request, TResult result) { }
    protected virtual Task OnQueryProcessed(TQuery request, TResult result) { }
    protected virtual Task OnQueryProcessedFromCache(TQuery request, TResult result) { }
    protected virtual Task OnQueryError(TQuery request, Exception exception) { }
    protected virtual string GetCacheKey(TQuery request) { }
    protected virtual bool ShouldCache(TQuery request) { }
    protected virtual int GetCacheDurationMinutes(TQuery request) { }
}
```

## DDD Alignment Features

### 🏗️ **Domain-Driven Design Principles**

#### **1. Ubiquitous Language**
- **Commands**: Represent intentions to change system state
- **Queries**: Represent read operations without side effects
- **Handlers**: Process commands and queries within application layer

#### **2. Bounded Context Separation**
- **Commands**: Modify state within specific bounded context
- **Queries**: Read data from specific bounded context
- **Clear boundaries**: Each handler operates within its context

#### **3. Aggregate Consistency**
- **Commands**: Maintain aggregate invariants and consistency
- **Queries**: Read data without affecting aggregate state
- **Event Publishing**: Commands can publish domain events

### 🔄 **CQRS Pattern Implementation**

#### **Command Side (Write)**
```csharp
// Command represents intention to change state
public class CreateUserCommand : IRequest<CreateUserResult>
{
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
}

// Command handler processes the command
public class CreateUserCommandHandler : CommandHandler<CreateUserCommand, CreateUserResult>
{
    protected override async Task<CreateUserResult> ProcessAsync(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic: create user, validate, save to database
        var user = UserFactory.CreateNewUser(
            request.Email, 
            request.FirstName, 
            request.LastName);

        await _userRepository.AddAsync(user, cancellationToken);
        
        return new CreateUserResult(user.Id);
    }
}
```

#### **Query Side (Read)**
```csharp
// Query represents read operation
public class GetUserByIdQuery : IQuery<UserDto>
{
    public Guid UserId { get; }
}

// Query handler processes the query
public class GetUserByIdQueryHandler : QueryHandler<GetUserByIdQuery, UserDto>
{
    protected override async Task<UserDto> ProcessAsync(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        // Business logic: retrieve user data
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        return _mapper.Map<UserDto>(user);
    }

    // Override caching for frequently accessed data
    protected override bool ShouldCache(GetUserByIdQuery request) => true;
    
    protected override string GetCacheKey(GetUserByIdQuery request) => 
        $"user_{request.UserId}";
}
```

## Implementation Examples

### 🎯 **Command Handler Examples**

#### **1. Simple Command (No Result)**
```csharp
public class DeleteUserCommand : IRequest
{
    public Guid UserId { get; }
    public string DeletedBy { get; }
}

public class DeleteUserCommandHandler : CommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    protected override async Task ProcessAsync(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Pure business logic - no cross-cutting concerns
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new UserNotFoundException(request.UserId);

        user.Delete(request.DeletedBy);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    protected override Task OnCommandReceived(DeleteUserCommand request)
    {
        _logger.LogInformation("Processing delete user command for user {UserId}", request.UserId);
        return Task.CompletedTask;
    }

    protected override Task ValidateCommand(DeleteUserCommand request)
    {
        if (request.UserId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty");

        if (string.IsNullOrWhiteSpace(request.DeletedBy))
            throw new ArgumentException("DeletedBy cannot be empty");

        return Task.CompletedTask;
    }
}
```

#### **2. Command with Result**
```csharp
public class CreateUserCommand : IRequest<CreateUserResult>
{
    public string Email { get; }
    public string Password { get; }
    public string FirstName { get; }
    public string LastName { get; }
}

public class CreateUserResult
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime CreatedAt { get; }
}

public class CreateUserCommandHandler : CommandHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task<CreateUserResult> ProcessAsync(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        // Pure business logic
        var email = Email.Create(request.Email);
        var password = Password.Create(request.Password);
        
        var user = UserFactory.CreateNewUser(
            email, 
            password, 
            request.FirstName, 
            request.LastName);

        await _userRepository.AddAsync(user, cancellationToken);

        // Send welcome email (side effect)
        await _emailService.SendWelcomeEmailAsync(user.Email.Value, cancellationToken);

        return new CreateUserResult
        {
            UserId = user.Id,
            Email = user.Email.Value,
            CreatedAt = user.CreatedAt ?? DateTime.UtcNow
        };
    }

    protected override Task OnCommandProcessed(CreateUserCommand request, CreateUserResult result)
    {
        _logger.LogInformation("Successfully created user {UserId} with email {Email}", 
            result.UserId, result.Email);
        return Task.CompletedTask;
    }
}
```

### 📊 **Query Handler Examples**

#### **1. Simple Query with Caching**
```csharp
public class GetUserProfileQuery : IQuery<UserProfileDto>
{
    public Guid UserId { get; }
}

public class GetUserProfileQueryHandler : QueryHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetUserProfileQueryHandler> _logger;

    public GetUserProfileQueryHandler(
        IUserRepository userRepository,
        ICacheService cacheService,
        ILogger<GetUserProfileQueryHandler> logger)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    protected override async Task<UserProfileDto> ProcessAsync(
        GetUserProfileQuery request, 
        CancellationToken cancellationToken)
    {
        // Pure business logic
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
            throw new UserNotFoundException(request.UserId);

        return new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Status = user.Status.Name
        };
    }

    // Enable caching for user profiles
    protected override bool ShouldCache(GetUserProfileQuery request) => true;

    protected override string GetCacheKey(GetUserProfileQuery request) => 
        $"user_profile_{request.UserId}";

    protected override int GetCacheDurationMinutes(GetUserProfileQuery request) => 30;

    protected override async Task<UserProfileDto?> TryGetFromCache(GetUserProfileQuery request)
    {
        var cacheKey = GetCacheKey(request);
        return await _cacheService.GetAsync<UserProfileDto>(cacheKey);
    }

    protected override async Task CacheResult(GetUserProfileQuery request, UserProfileDto result)
    {
        var cacheKey = GetCacheKey(request);
        var duration = TimeSpan.FromMinutes(GetCacheDurationMinutes(request));
        await _cacheService.SetAsync(cacheKey, result, duration);
    }
}
```

#### **2. Complex Query with Pagination**
```csharp
public class GetUsersQuery : IQuery<PaginatedResult<UserDto>>
{
    public int Page { get; }
    public int PageSize { get; }
    public string? SearchTerm { get; }
    public string? Status { get; }
}

public class GetUsersQueryHandler : QueryHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUsersQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    protected override async Task<PaginatedResult<UserDto>> ProcessAsync(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        // Pure business logic
        var specification = new UsersSpecification
        {
            SearchTerm = request.SearchTerm,
            Status = request.Status,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var users = await _userRepository.GetBySpecificationAsync(specification, cancellationToken);
        var totalCount = await _userRepository.CountBySpecificationAsync(specification, cancellationToken);

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email.Value,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Status = u.Status.Name
        }).ToList();

        return new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    protected override Task ValidateQuery(GetUsersQuery request)
    {
        if (request.Page < 1)
            throw new ArgumentException("Page must be greater than 0");

        if (request.PageSize < 1 || request.PageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100");

        return Task.CompletedTask;
    }
}
```

## Method Flow Patterns

### 🔄 **Command Handler Flow**

```
1. MediatR calls Handle(request, cancellationToken)
   ↓
2. OnCommandReceived(request) - Logging
   ↓
3. ValidateCommand(request) - Validation
   ↓
4. ProcessAsync(request, cancellationToken) - Business Logic
   ↓
5. OnCommandProcessed(request, result) - Success Logging
   ↓
6. Return to MediatR
```

### 📊 **Query Handler Flow**

```
1. MediatR calls Handle(request, cancellationToken)
   ↓
2. OnQueryReceived(request) - Logging
   ↓
3. ValidateQuery(request) - Validation
   ↓
4. TryGetFromCache(request) - Cache Check
   ↓
5a. If cached: OnQueryProcessedFromCache(request, result) - Cache Hit
   ↓
5b. If not cached: ProcessAsync(request, cancellationToken) - Business Logic
   ↓
6b. CacheResult(request, result) - Cache Result
   ↓
7. OnQueryProcessed(request, result) - Success Logging
   ↓
8. Return to MediatR
```

## DDD Best Practices

### ✅ **Command Handler Best Practices**

1. **Single Responsibility**: Each command should represent one business operation
2. **Idempotency**: Commands should be idempotent when possible
3. **Validation**: Validate commands before processing
4. **Domain Events**: Publish domain events for state changes
5. **Transaction Management**: Use unit of work pattern for consistency

### ✅ **Query Handler Best Practices**

1. **Read-Only**: Queries should not modify system state
2. **Caching**: Implement appropriate caching strategies
3. **Performance**: Optimize queries for read performance
4. **Pagination**: Use pagination for large result sets
5. **Projection**: Return DTOs, not domain entities

### 🚫 **Common Mistakes**

1. **Mixing Commands and Queries**: Don't modify state in queries
2. **Ignoring Validation**: Always validate commands and queries
3. **Poor Error Handling**: Implement proper error handling and logging
4. **No Caching**: Don't ignore caching opportunities for queries
5. **Complex Business Logic**: Keep handlers focused and simple

## Benefits

### 🎯 **DDD Alignment**

1. **Ubiquitous Language**: Clear, domain-specific terminology
2. **Bounded Contexts**: Proper separation of concerns
3. **Aggregate Consistency**: Maintain data integrity
4. **Domain Events**: Proper event-driven architecture

### 🔧 **Professional Quality**

1. **Consistent Patterns**: All handlers follow the same structure
2. **Cross-Cutting Concerns**: Centralized logging, validation, error handling
3. **Testability**: Business logic can be tested independently
4. **Maintainability**: Clear separation of responsibilities

### 🚀 **Performance**

1. **Caching Support**: Built-in caching for queries
2. **Async Operations**: Full async/await support
3. **Cancellation**: Proper cancellation token support
4. **Optimization**: Efficient processing patterns

## Conclusion

The enhanced CommandHandler and QueryHandler classes provide:

- **DDD Alignment**: Proper domain-driven design implementation
- **CQRS Support**: Clear separation of commands and queries
- **Professional Patterns**: Enterprise-grade implementation
- **Performance Optimization**: Built-in caching and async support
- **Maintainability**: Consistent patterns and clear responsibilities

This implementation ensures that your application follows DDD principles while maintaining high performance and professional code quality. 