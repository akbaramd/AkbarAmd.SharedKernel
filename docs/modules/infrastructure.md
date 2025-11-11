# AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore

The Infrastructure layer provides implementations for data access, external services, and framework integrations. This module specifically implements Entity Framework Core repositories and specification evaluators for database operations.

## Overview

This module provides:
- **Repository implementations** using Entity Framework Core
- **Specification evaluators** for translating domain specifications to EF Core queries
- **Database context extensions** for common configurations
- **Data access abstractions** implementations

## Key Components

### Repository Implementations

- `ReadOnlyEfRepository<TDbContext, T, TKey>`: Read-only repository base class
- `EfRepository<TDbContext, T, TKey>`: Full repository with CRUD operations
- Supports both tracking and no-tracking queries
- Built-in pagination and sorting

### Specification Evaluator

- `EfCoreSpecificationEvaluator<T>`: Translates domain specifications to EF Core queries
- Supports filtering, sorting, pagination, and includes
- Optimized query generation

### EF Core Extensions

- `ModelBuilderExtensions`: Extension methods for model configuration
- `EntityTypeBuilderExtensions`: Extension methods for entity configuration

## Features

- **Repository Pattern**: Generic repository with specification support
- **EF Core Integration**: Seamless Entity Framework Core integration
- **Query Optimization**: Efficient query generation with tracking control
- **Specification Support**: Translate domain specifications to database queries
- **Pagination**: Built-in pagination support
- **Performance**: Support for split queries and query tags

## Installation

```bash
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Usage Example

### Creating a Repository

```csharp
public class UserRepository : EfRepository<ApplicationDbContext, User, Guid>
{
    public UserRepository(ApplicationDbContext dbContext) 
        : base(dbContext, enableTracking: false)
    {
    }

    // Custom query methods can be added here
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
```

### Using Specifications with Repository

```csharp
// Define specification
var activeUsersSpec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .Take(10)
    .Build();

// Use with repository
var users = await _userRepository.GetAsync(activeUsersSpec, cancellationToken);
```

## Related Concepts

- [Repository](concepts/repository.md) - Understand the repository pattern
- [Specifications](concepts/specifications.md) - Learn how to use specifications
- [Aggregate Root](concepts/aggregate-root.md) - Work with aggregate roots

## Dependencies

- **Entity Framework Core** 8.0: ORM framework
- **Microsoft.AspNetCore.Authorization**: Authorization support
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore**: Identity integration

## Performance Considerations

- **No-Tracking by Default**: Read-only operations use `AsNoTracking()` for better performance
- **Split Queries**: Support for split queries to avoid cartesian explosion
- **Query Tags**: Support for query tags for debugging and monitoring
- **Identity Resolution**: Configurable identity resolution for no-tracking queries

