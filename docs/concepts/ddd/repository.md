# Repository Pattern

The Repository pattern provides an abstraction layer between the domain and data access layers, encapsulating the logic needed to access data sources.

## Installation

To use the Repository pattern, install the following modules:

```bash
# Core domain module (required for repository interfaces)
dotnet add package AkbarAmd.SharedKernel.Domain

# EF Core infrastructure (required for repository implementations)
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Overview

Repositories centralize common data access functionality and provide a more object-oriented view of the persistence layer. They also make it easier to test your business logic by allowing you to mock the repository.

## Key Interfaces

### IReadOnlyRepository<T, TKey>

Provides read-only access to entities:

```csharp
public interface IReadOnlyRepository<T, TKey>
    where T : Entity<TKey>
    where TKey : IEquatable<TKey>
{
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAsync(ISpecification<T>? specification = null, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> GetPaginatedAsync(int pageNumber, int pageSize, ISpecification<T>? specification = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ISpecification<T>? specification = null, CancellationToken cancellationToken = default);
}
```

### IRepository<T, TKey>

Extends `IReadOnlyRepository` with write operations:

```csharp
public interface IRepository<T, TKey> : IReadOnlyRepository<T, TKey>
    where T : Entity<TKey>
    where TKey : IEquatable<TKey>
{
    Task AddAsync(T entity, bool save = false, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, bool save = false, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, bool save = false, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);
    Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
```

## Implementation

The infrastructure module provides EF Core implementations:

- `EfRepository<TDbContext, T, TKey>`: Full repository implementation
- `ReadOnlyEfRepository<TDbContext, T, TKey>`: Read-only repository implementation

## Usage Examples

### Basic CRUD Operations

```csharp
public class UserService
{
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IRepository<User, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> CreateUserAsync(string email, string name, CancellationToken cancellationToken)
    {
        var user = new User(email, name);
        await _userRepository.AddAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
        return user.Id;
    }

    public async Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(userId, cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            await _userRepository.DeleteAsync(user, cancellationToken: cancellationToken);
            await _userRepository.SaveAsync(cancellationToken);
        }
    }
}
```

### Using with Specifications

```csharp
public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken)
{
    var spec = new CriteriaBuilder<User>()
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .Build();

    return await _userRepository.GetAsync(spec, cancellationToken);
}
```

### Pagination

```csharp
public async Task<PaginatedResult<User>> GetUsersPaginatedAsync(
    int pageNumber, 
    int pageSize, 
    CancellationToken cancellationToken)
{
    var spec = new CriteriaBuilder<User>()
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .Build();

    return await _userRepository.GetPaginatedAsync(pageNumber, pageSize, spec, cancellationToken);
}
```

### Batch Operations

```csharp
public async Task CreateMultipleUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken)
{
    await _userRepository.AddRangeAsync(users, cancellationToken: cancellationToken);
    await _userRepository.SaveAsync(cancellationToken);
}
```

## Benefits

- **Abstraction**: Hides data access implementation details
- **Testability**: Easy to mock for unit testing
- **Flexibility**: Can swap implementations (EF Core, Dapper, MongoDB, etc.)
- **Consistency**: Standardized data access patterns across the application
- **Specification Support**: Works seamlessly with the specification pattern

## Related Topics

- [Specifications](specifications.md) - Build complex queries with specifications
- [Aggregate Root](aggregate-root.md) - Work with aggregate roots in repositories
- [DDD Overview](index.md) - Learn about DDD concepts
- [Infrastructure Module](../../modules/infrastructure.md) - See EF Core repository implementations

