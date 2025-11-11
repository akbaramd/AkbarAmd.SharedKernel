# Specifications Pattern

The Specification pattern encapsulates business rules and query logic in reusable, composable objects. It provides a clean way to express complex queries while keeping them testable and maintainable.

## Installation

To use the Specifications pattern, install the following modules:

```bash
# Core domain module (required for specification interfaces and builders)
dotnet add package AkbarAmd.SharedKernel.Domain

# EF Core infrastructure (required for specification evaluation)
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Overview

Specifications allow you to:
- Encapsulate query logic in reusable objects
- Compose complex queries from simple specifications
- Test query logic independently
- Keep domain logic separate from data access concerns

## Key Interface

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
    IReadOnlyList<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}
```

## Building Specifications

### Using CriteriaBuilder

The `CriteriaBuilder<T>` provides a fluent API for building specifications:

```csharp
var spec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .Where(u => u.Email.Contains("@example.com"))
    .OrderBy(u => u.CreatedAt)
    .Take(10)
    .Build();
```

### Basic Filtering

```csharp
// Simple filter
var activeUsersSpec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .Build();

var users = await repository.GetAsync(activeUsersSpec);
```

### Multiple Conditions

```csharp
// Multiple WHERE clauses are combined with AND
var spec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .Where(u => u.CreatedAt > DateTime.UtcNow.AddDays(-30))
    .Where(u => u.Email != null)
    .Build();
```

### Sorting

```csharp
// Order by ascending
var spec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .Build();

// Order by descending
var spec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .OrderByDescending(u => u.CreatedAt)
    .Build();
```

### Pagination

```csharp
var spec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .Skip(20)
    .Take(10)
    .Build();
```

### Including Related Entities

```csharp
var spec = new CriteriaBuilder<Order>()
    .Where(o => o.Status == OrderStatus.Pending)
    .Include(o => o.Customer)
    .Include(o => o.Items)
    .Build();
```

### Complex Queries

```csharp
var spec = new CriteriaBuilder<Product>()
    .Where(p => p.Price > 100)
    .Where(p => p.Category.Name == "Electronics")
    .Include(p => p.Category)
    .Include(p => p.Supplier)
    .OrderByDescending(p => p.Price)
    .Take(20)
    .Build();
```

## Using Specifications with Repositories

```csharp
public class UserService
{
    private readonly IRepository<User, Guid> _userRepository;

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken)
    {
        var spec = new CriteriaBuilder<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .Build();

        return await _userRepository.GetAsync(spec, cancellationToken);
    }

    public async Task<PaginatedResult<User>> GetUsersPaginatedAsync(
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        var spec = new CriteriaBuilder<User>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.CreatedAt)
            .Build();

        return await _userRepository.GetPaginatedAsync(
            pageNumber, 
            pageSize, 
            spec, 
            cancellationToken);
    }
}
```

## Creating Custom Specifications

You can also create custom specification classes:

```csharp
public class ActiveUsersSpecification : ISpecification<User>
{
    public Expression<Func<User, bool>>? Criteria => u => u.IsActive;
    public IReadOnlyList<Expression<Func<User, object>>> Includes => Array.Empty<Expression<Func<User, object>>>();
    public IReadOnlyList<string> IncludeStrings => Array.Empty<string>();
    public Expression<Func<User, object>>? OrderBy => u => u.CreatedAt;
    public Expression<Func<User, object>>? OrderByDescending => null;
    public int Take => 0;
    public int Skip => 0;
    public bool IsPagingEnabled => false;
}

// Usage
var spec = new ActiveUsersSpecification();
var users = await repository.GetAsync(spec);
```

## Benefits

- **Reusability**: Specifications can be reused across different parts of the application
- **Testability**: Easy to unit test query logic
- **Composability**: Can combine multiple specifications
- **Separation of Concerns**: Query logic is separate from data access code
- **Type Safety**: Compile-time checking of query expressions

## Best Practices

1. **Keep specifications focused**: Each specification should represent a single business rule or query
2. **Use CriteriaBuilder for dynamic queries**: Use the fluent API for queries that change based on user input
3. **Create custom specifications for complex logic**: For complex, reusable queries, create dedicated specification classes
4. **Test specifications independently**: Write unit tests for your specifications
5. **Use includes wisely**: Only include related entities when necessary to avoid performance issues

## Related Topics

- [Repository](repository.md) - Use specifications with repositories
- [DDD Overview](index.md) - Learn about DDD concepts
- [Infrastructure Module](../../modules/infrastructure.md) - See how specifications are evaluated in EF Core

