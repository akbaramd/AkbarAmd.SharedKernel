# Getting Started

## Introduction

The Specification Pattern is a Domain-Driven Design (DDD) pattern that encapsulates business rules and query logic into reusable, testable components. This implementation provides a fluent API for building complex queries while maintaining clean architecture principles.

## Installation

The Specification Pattern is included in the `AkbarAmd.SharedKernel.Domain` package:

```bash
dotnet add package AkbarAmd.SharedKernel.Domain
```

For Entity Framework Core integration:

```bash
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Basic Example

### Using FluentSpecificationBuilder

The simplest way to create a specification is using the fluent builder:

```csharp
using AkbarAmd.SharedKernel.Domain.Specifications;

var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Page(1, 10)
    .Build();

var products = await repository.FindAsync(spec);
```

### Using BaseSpecification

For reusable specifications, inherit from `BaseSpecification<T>`:

```csharp
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}

// Usage
var spec = new ActiveProductsSpecification();
var products = await repository.FindAsync(spec);
```

## Key Concepts

### 1. Criteria (Filtering)

Criteria define which entities match the specification:

```csharp
.Where(p => p.IsActive)
.Where(p => p.Price > 100)
```

### 2. Sorting

Control the order of results:

```csharp
.OrderBy(p => p.Name)
.ThenByDescending(p => p.Price)
```

### 3. Pagination

Efficiently page through large datasets:

```csharp
.Page(pageNumber: 1, pageSize: 10)
```

### 4. Includes (Eager Loading)

Load related entities:

```csharp
.Include(p => p.Category)
.Include(p => p.Reviews)
```

## Next Steps

- Learn about [Basic Usage](basic-usage.md)
- Explore the [Fluent API](fluent-api.md)
- See [Database Patterns](database-patterns.md) for enterprise scenarios

