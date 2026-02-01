# ServiceResult Pattern

The ServiceResult pattern provides a standardized, type-safe way to represent operation outcomes without using exceptions for expected failures. This pattern enables functional error handling and makes error conditions explicit in method signatures.

## Overview

`ServiceResult` and `ServiceResult<T>` are immutable result types that encapsulate:
- **Operation status** (Success, Failure, ValidationFailed, NotFound, etc.)
- **Error information** (Messages, error codes, detailed errors)
- **Metadata** (Trace IDs, timestamps, custom metadata)
- **Return values** (for generic `ServiceResult<T>`)

## Key Benefits

- **Type Safety**: Errors are part of the return type, not hidden exceptions
- **Explicit Error Handling**: Forces developers to handle failure cases
- **Functional Programming**: Supports monadic operations (Map, Bind)
- **Rich Error Information**: Structured error data with categories and targets
- **Traceability**: Built-in trace ID correlation for distributed systems

## Basic Usage

### Creating Results

```csharp
// Success without value
var result = ServiceResult.Ok("Operation completed successfully");

// Success with value
var userResult = ServiceResult<User>.Ok(user, "User retrieved successfully");

// Failure
var errorResult = ServiceResult.Fail("User not found", code: "USER_NOT_FOUND");

// Specific error types
var notFound = ServiceResult.NotFound("User with ID 123 not found", target: "userId");
var unauthorized = ServiceResult.Unauthorized("Invalid credentials");
var conflict = ServiceResult.Conflict("Email already exists", target: "email");
```

### Checking Results

```csharp
var result = await GetUserAsync(userId);

if (result.IsSuccess)
{
    var user = result.Value;
    // Use user...
}
else
{
    // Handle error
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.Code}: {error.Message}");
    }
}
```

### Pattern Matching

```csharp
var result = await GetUserAsync(userId);

var message = result switch
{
    { IsSuccess: true } => $"User: {result.Value.Name}",
    { Status: ServiceResultStatus.NotFound } => "User not found",
    { Status: ServiceResultStatus.Unauthorized } => "Access denied",
    _ => $"Error: {result.Message}"
};
```

## Functional Programming Patterns

### Map (Functor)

Transform the value of a successful result:

```csharp
var userResult = await GetUserAsync(userId);

// Transform User to UserDto
var dtoResult = userResult.Map(user => new UserDto
{
    Id = user.Id,
    Name = user.Name,
    Email = user.Email
});

// If userResult is a failure, dtoResult will have the same error
// If userResult is success, dtoResult will contain the transformed DTO
```

### Bind (Monad)

Chain operations that return service results:

```csharp
var userResult = await GetUserAsync(userId);

// Chain to get user's orders
var ordersResult = userResult.Bind(user => GetUserOrdersAsync(user.Id));

// If userResult fails, ordersResult will have the same error
// If userResult succeeds, GetUserOrdersAsync is called with the user
```

### Chaining Operations

```csharp
var result = await GetUserAsync(userId)
    .Map(user => user.Email)
    .Bind(email => ValidateEmailAsync(email))
    .Bind(validated => SendEmailAsync(validated));
```

## Status Types

The `ServiceResultStatus` enum provides specific status values:

- **Success**: Operation completed successfully
- **Failure**: General operation failure
- **ValidationFailed**: Input validation errors
- **NotFound**: Requested resource not found
- **Unauthorized**: Authentication required
- **Forbidden**: Insufficient permissions
- **Conflict**: State conflict (e.g., duplicate entry)
- **Unavailable**: Service temporarily unavailable
- **Timeout**: Operation timed out
- **Cancelled**: Operation was cancelled

## Error Details

Each result can contain multiple `ServiceError` objects with:

- **Code**: Machine-readable error code
- **Message**: Human-readable error message
- **Category**: Error category (Validation, NotFound, etc.)
- **Target**: Field/property/resource identifier
- **AttemptedValue**: The value that caused the error (for validation)

```csharp
var errors = new[]
{
    ServiceError.Validation("REQUIRED", "Email is required", target: "email"),
    ServiceError.Validation("INVALID_FORMAT", "Invalid email format", 
        target: "email", attemptedValue: "invalid-email")
};

var result = ServiceResult.Fail("Validation failed", errors: errors);
```

## Exception Handling

Convert exceptions to service results:

```csharp
try
{
    var user = await repository.GetByIdAsync(userId);
    return ServiceResult<User>.Ok(user);
}
catch (Exception ex)
{
    return ServiceResult<User>.FromException(ex);
}
```

Common exception types are automatically mapped:
- `KeyNotFoundException` → `NotFound`
- `UnauthorizedAccessException` → `Unauthorized`
- `ArgumentException` → `ValidationFailed`
- `OperationCanceledException` → `Cancelled`
- `TimeoutException` → `Timeout`

## FluentValidation Integration

Convert FluentValidation results directly:

```csharp
var validator = new CreateUserCommandValidator();
var validationResult = await validator.ValidateAsync(command);

if (!validationResult.IsValid)
{
    return ServiceResult.Validation(validationResult);
}

// Continue with operation...
```

## Pagination Support

Create paginated results:

```csharp
var users = await repository.GetPagedAsync(pageNumber: 1, pageSize: 10);
var totalCount = await repository.CountAsync();

return ServiceResult.OkPaged(users, pageNumber: 1, pageSize: 10, totalCount);
```

## Async Conversions

Service results can be implicitly converted to tasks:

```csharp
// These are equivalent:
Task<ServiceResult> task1 = ServiceResult.Ok();
Task<ServiceResult> task2 = ServiceResult.Ok().AsTask();

// Works with async methods
public async Task<ServiceResult<User>> GetUserAsync(Guid id)
{
    // ... implementation
    return ServiceResult<User>.Ok(user); // Automatically wrapped in Task
}
```

## Metadata and Traceability

Add custom metadata and trace IDs:

```csharp
var result = ServiceResult.Ok()
    .WithTraceId(correlationId)
    .AddMetadata("RequestId", requestId)
    .AddMetadata("UserId", userId.ToString());
```

## Best Practices

1. **Always Check Results**: Don't ignore `IsSuccess` checks
2. **Use Specific Status Types**: Prefer `NotFound()` over `Fail()` when appropriate
3. **Provide Error Codes**: Use meaningful error codes for programmatic handling
4. **Chain Operations**: Use Map/Bind for functional composition
5. **Handle Exceptions**: Use `TryAsync` or `FromException` for exception handling
6. **Preserve Trace IDs**: Use `WithTraceId` for distributed tracing

## Related Topics

- [ServiceBase](service-base.md) - Base class for service implementations
- [CQRS](../cqrs/cqrs.md) - Command Query Responsibility Segregation
- [Application Module](../../modules/application.md) - Application layer overview
