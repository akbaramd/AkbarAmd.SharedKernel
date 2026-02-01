# ServiceBase

`ServiceBase` is an abstract base class that simplifies service implementation by providing convenient helper methods for creating and handling `ServiceResult` instances with consistent trace ID correlation.

## Overview

ServiceBase provides:
- **Factory methods** for creating service results with automatic trace ID handling
- **Async helpers** for asynchronous operations
- **Exception handling** utilities for safe operation execution
- **Functional transformations** (Map, Bind) for result composition
- **Validation helpers** for condition checking

## Key Features

- **Automatic Trace ID Correlation**: All results include trace IDs for distributed tracing
- **Consistent Error Handling**: Standardized methods for common error scenarios
- **Functional Programming Support**: Map and Bind methods for result composition
- **Exception Safety**: TryAsync methods for safe exception handling
- **FluentValidation Integration**: Built-in support for validation results

## Basic Usage

### Inheriting from ServiceBase

```csharp
public class UserService : ServiceBase
{
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<User>> GetUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return NotFound<User>("User not found", target: userId.ToString());
        
        return Ok(user, "User retrieved successfully");
    }
}
```

### Custom Trace ID Handling

Override `DefaultTraceId` to provide custom trace ID logic:

```csharp
public class UserService : ServiceBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override string? DefaultTraceId 
        => _httpContextAccessor.HttpContext?.TraceIdentifier;
}
```

## Result Creation Methods

### Success Results

```csharp
// Without value
var result = Ok("Operation completed");

// With value
var userResult = Ok(user, "User retrieved successfully");

// Async versions
var asyncResult = await OkAsync("Operation completed");
var asyncUserResult = await OkAsync(user, "User retrieved");
```

### Failure Results

```csharp
// General failure
var error = Fail("Operation failed", code: "OPERATION_FAILED");

// Specific error types
var notFound = NotFound("User not found", target: "userId");
var unauthorized = Unauthorized("Invalid credentials");
var forbidden = Forbidden("Insufficient permissions");
var conflict = Conflict("Email already exists", target: "email");

// With exception
var errorWithEx = Fail("Database error", exception: ex);
```

### Validation Results

```csharp
var validator = new CreateUserCommandValidator();
var validationResult = await validator.ValidateAsync(command);

if (!validationResult.IsValid)
{
    return Validation(validationResult, "Validation failed");
}
```

### Exception Handling

```csharp
// Convert exception to result
var result = FromException(ex);

// Typed version
var userResult = FromException<User>(ex);
```

## Async Helpers

All synchronous methods have async counterparts that return `Task<ServiceResult>`:

```csharp
// Synchronous
var result = Ok("Success");

// Asynchronous (wrapped in Task)
var asyncResult = await OkAsync("Success");
```

## Exception Safety with TryAsync

Execute operations safely, automatically converting exceptions to results:

### No Return Value

```csharp
var result = await TryAsync(async ct =>
{
    await _repository.DeleteAsync(userId, ct);
}, cancellationToken);

if (result.IsSuccess)
{
    // Deletion succeeded
}
```

### With Return Value

```csharp
var result = await TryAsync(async ct =>
{
    return await _repository.GetByIdAsync(userId, ct);
}, cancellationToken);

if (result.IsSuccess)
{
    var user = result.Value;
    // Use user...
}
```

### With Service Result Return

```csharp
var result = await TryAsync(async ct =>
{
    var user = await _repository.GetByIdAsync(userId, ct);
    if (user == null)
        return NotFound<User>("User not found");
    
    return Ok(user);
}, cancellationToken);
```

## Functional Transformations

### Map

Transform successful results:

```csharp
var userResult = await GetUserAsync(userId);

// Transform User to UserDto
var dtoResult = Map(userResult, user => new UserDto
{
    Id = user.Id,
    Name = user.Name,
    Email = user.Email
}, successMessage: "User DTO retrieved");
```

### Bind

Chain operations that return service results:

```csharp
var userResult = await GetUserAsync(userId);

// Chain to get user's orders
var ordersResult = Bind(userResult, user => GetUserOrdersAsync(user.Id));
```

## Validation Helpers

### Ensure

Validate conditions and return appropriate results:

```csharp
// Without value
var result = Ensure(
    user != null,
    "User not found",
    code: "USER_NOT_FOUND",
    target: userId.ToString()
);

// With typed value
var userResult = Ensure<User>(
    user != null,
    "User not found",
    code: "USER_NOT_FOUND"
);
```

## Complete Example

```csharp
public class UserService : ServiceBase
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IValidator<CreateUserCommand> _validator;

    public UserService(
        IRepository<User, Guid> userRepository,
        IValidator<CreateUserCommand> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<ServiceResult<User>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Validation<User>(validationResult);
        }

        // Check for duplicate email
        var existingUser = await _userRepository.FindAsync(
            u => u.Email == command.Email,
            cancellationToken);

        if (existingUser != null)
        {
            return Conflict<User>(
                "Email already exists",
                target: "email");
        }

        // Create user safely
        return await TryAsync(async ct =>
        {
            var user = new User(command.Email, command.Name);
            await _userRepository.AddAsync(user, ct);
            return Ok(user, "User created successfully");
        }, cancellationToken);
    }

    public async Task<ServiceResult<UserDto>> GetUserDtoAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userResult = await TryAsync(async ct =>
        {
            var user = await _userRepository.GetByIdAsync(userId, ct);
            if (user == null)
                return NotFound<User>("User not found");
            
            return Ok(user);
        }, cancellationToken);

        // Transform to DTO
        return Map(userResult, user => new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }
}
```

## Best Practices

1. **Inherit from ServiceBase**: Use it as a base class for all service implementations
2. **Override DefaultTraceId**: Provide custom trace ID logic when needed
3. **Use TryAsync**: Wrap operations that might throw exceptions
4. **Use Specific Error Types**: Prefer `NotFound()` over `Fail()` when appropriate
5. **Chain Operations**: Use Map and Bind for functional composition
6. **Validate Early**: Use Ensure or Validation methods for input validation
7. **Preserve Context**: Always use service base methods to maintain trace IDs

## Related Topics

- [ServiceResult](service-result.md) - Service result pattern overview
- [CQRS](../cqrs/cqrs.md) - Command Query Responsibility Segregation
- [Application Module](../../modules/application.md) - Application layer overview
