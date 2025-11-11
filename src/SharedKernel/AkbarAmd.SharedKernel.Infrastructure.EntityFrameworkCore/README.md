# AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore

Clean Architecture Infrastructure Layer - EF Core Integration, Repository Implementations, and Data Access

## Overview

The Infrastructure layer provides implementations for data access, external services, and framework integrations. This package specifically implements Entity Framework Core repositories and specification evaluators for database operations.

## Purpose

This layer provides:
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

## Usage Examples

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

### Repository Operations

```csharp
// Add entity
var user = new User("user@example.com", "John Doe");
await _userRepository.AddAsync(user, cancellationToken);
await _userRepository.SaveChangesAsync(cancellationToken);

// Update entity
user.UpdateName("Jane Doe");
await _userRepository.UpdateAsync(user, cancellationToken);
await _userRepository.SaveChangesAsync(cancellationToken);

// Delete entity
await _userRepository.DeleteAsync(userId, cancellationToken);
await _userRepository.SaveChangesAsync(cancellationToken);

// Get by ID
var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

// Get all with pagination
var paginatedResult = await _userRepository.GetPaginatedAsync(
    pageNumber: 1,
    pageSize: 10,
    cancellationToken: cancellationToken);
```

### Configuring DbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Configure audit properties
        modelBuilder.ConfigureAuditProperties();
    }
}
```

## Dependencies

- **Entity Framework Core** 8.0: ORM framework
- **Microsoft.AspNetCore.Authorization**: Authorization support
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore**: Identity integration

## Architecture Principles

This layer follows Clean Architecture principles:
- **Implements Domain abstractions**: Provides concrete implementations for repository interfaces
- **Framework-specific**: Contains EF Core-specific code
- **Isolated from Domain**: Domain layer has no knowledge of this implementation
- **Testable**: Can be replaced with in-memory implementations for testing

## Performance Considerations

- **No-Tracking by Default**: Read-only operations use `AsNoTracking()` for better performance
- **Split Queries**: Support for split queries to avoid cartesian explosion
- **Query Tags**: Support for query tags for debugging and monitoring
- **Identity Resolution**: Configurable identity resolution for no-tracking queries

## License

MIT License - see LICENSE file for details.

## Author

Akbar Ahmadi Saray - [GitHub](https://github.com/akbarahmadi)
