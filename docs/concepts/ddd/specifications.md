# Specifications Pattern

The Specification pattern encapsulates business rules and query criteria in reusable, composable objects. It provides a clean, type-safe way to express complex queries while keeping them testable and maintainable.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [Creating Specifications](#creating-specifications)
  - [FluentSpecificationBuilder](#fluentspecificationbuilder)
  - [BaseSpecification (Custom Classes)](#basespecification-custom-classes)
- [Building Criteria](#building-criteria)
  - [Simple Criteria](#simple-criteria)
  - [AND Conditions](#and-conditions)
  - [OR Conditions](#or-conditions)
  - [NOT Conditions](#not-conditions)
  - [Grouping Conditions](#grouping-conditions)
  - [Complex Criteria](#complex-criteria)
- [Combining Specifications](#combining-specifications)
  - [AND Combination](#and-combination)
  - [OR Combination](#or-combination)
  - [NOT Combination](#not-combination)
  - [Chaining Combinations](#chaining-combinations)
  - [AllOf and AnyOf Helpers](#allof-and-anyof-helpers)
  - [Combining with Expressions](#combining-with-expressions)
- [Using with Repositories](#using-with-repositories)
  - [Basic Queries](#basic-queries)
  - [Pagination](#pagination)
  - [Sorting](#sorting)
  - [Counting and Existence Checks](#counting-and-existence-checks)
- [In-Memory Validation](#in-memory-validation)
- [Complete Examples](#complete-examples)
- [Best Practices](#best-practices)

## Overview

Specifications in this implementation follow the **Domain-Driven Design (DDD)** Specification pattern:

- **Pure Domain Logic**: Specifications contain only business rules (criteria), no infrastructure concerns
- **Composable**: Combine multiple specifications using AND, OR, and NOT operations
- **Immutable**: All operations return new specifications without modifying originals
- **Testable**: Use `IsSatisfiedBy()` for in-memory validation
- **Query-Ready**: Use `ToExpression()` or `Criteria` property for repository queries

### Key Principles

1. **Criteria Only**: Specifications define **what** to filter, not **how** to query
2. **Infrastructure Separation**: Sorting, pagination, and includes are handled at the repository level
3. **Reusability**: Build specifications once, use them everywhere
4. **Type Safety**: Compile-time checking of all expressions

## Quick Start

```csharp
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;

// Create a specification using FluentSpecificationBuilder
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Where(p => p.Price > 100m)
    .Build();

// Use with repository
var products = await repository.FindAsync(spec);

// Or combine multiple specifications
var activeSpec = new ActiveProductsSpecification();
var categorySpec = new ProductsByCategorySpecification("Electronics");
var combined = activeSpec.And(categorySpec);
var results = await repository.FindAsync(combined);
```

## Creating Specifications

There are two ways to create specifications:

### FluentSpecificationBuilder

Use `FluentSpecificationBuilder<T>` for ad-hoc, one-off specifications:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Build();
```

**When to use**: Dynamic queries, temporary filters, or when you don't need a reusable specification class.

### BaseSpecification (Custom Classes)

Create custom specification classes for reusable business rules:

```csharp
public sealed class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}
```

**When to use**: Named business rules that appear in your domain's ubiquitous language (e.g., "Active Products", "Overdue Invoices").

## Building Criteria

### Simple Criteria

Add a single condition:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Build();
```

### AND Conditions

Multiple `Where()` calls are combined with AND:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Where(p => p.Category == "Electronics")
    .Where(p => p.Price > 100m)
    .Build();
// Result: p => p.IsActive && p.Category == "Electronics" && p.Price > 100m
```

### OR Conditions

Use the fluent chain API with `Or()`:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Or(p => p.Category == "Electronics")
    .Build();
// Result: p => p.IsActive || p.Category == "Electronics"
```

### NOT Conditions

Use `Not()` in the fluent chain:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Not(p => p.IsDeleted)
    .Build();
// Result: p => p.IsActive && !p.IsDeleted
```

### Grouping Conditions

Use `Group()` to create AND groups, or `OrGroup()` for OR groups:

```csharp
// Group with AND: (A AND B) AND (existing root)
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Group(g => g
        .And(p => p.Category == "Electronics")
        .And(p => p.Price > 100m))
    .Build();
// Result: p => p.IsActive && (p.Category == "Electronics" && p.Price > 100m)

// OrGroup with OR: (A AND B) OR (existing root)
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrGroup(g => g
        .And(p => p.Category == "Electronics")
        .And(p => p.Price > 100m))
    .Build();
// Result: p => p.IsActive || (p.Category == "Electronics" && p.Price > 100m)
```

### Complex Criteria

Build complex nested conditions using builder functions:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(b => b
        .Group(g => g
            .And(p => p.IsActive)
            .And(p => p.Price > 50m))
        .Or(p => p.Category == "Electronics"))
    .Build();
// Result: p => (p.IsActive && p.Price > 50m) || p.Category == "Electronics"
```

**Complete Fluent API Reference:**

| Method | Description | Example |
|--------|-------------|---------|
| `Where(expr)` | Add a simple condition (AND with previous) | `.Where(p => p.IsActive)` |
| `Where(builder)` | Add complex criteria using builder | `.Where(b => b.And(...).Or(...))` |
| `.And(expr)` | Add AND condition to chain | `.Where(p => p.IsActive).And(p => p.Price > 100)` |
| `.Or(expr)` | Add OR condition to chain | `.Where(p => p.IsActive).Or(p => p.Category == "X")` |
| `.Not(expr)` | Add NOT condition to chain | `.Where(p => p.IsActive).Not(p => p.IsDeleted)` |
| `.Group(builder)` | Create AND group, combine with AND | `.Group(g => g.And(A).And(B))` |
| `.OrGroup(builder)` | Create AND group, combine with OR | `.OrGroup(g => g.And(A).And(B))` |
| `Build()` | Finalize and return specification | `.Build()` |

## Combining Specifications

Combine multiple `ISpecification<T>` instances to create more complex specifications.

### AND Combination

Combine two specifications with AND logic:

```csharp
var activeSpec = new ActiveProductsSpecification();
var categorySpec = new ProductsByCategorySpecification("Electronics");
var combined = activeSpec.And(categorySpec);
// Result: Products that are active AND in Electronics category
```

### OR Combination

Combine two specifications with OR logic:

```csharp
var electronicsSpec = new ProductsByCategorySpecification("Electronics");
var furnitureSpec = new ProductsByCategorySpecification("Furniture");
var combined = electronicsSpec.Or(furnitureSpec);
// Result: Products in Electronics OR Furniture category
```

### NOT Combination

Negate a specification:

```csharp
var activeSpec = new ActiveProductsSpecification();
var inactiveSpec = activeSpec.Not();
// Result: Products that are NOT active
```

### Chaining Combinations

Chain multiple combinations:

```csharp
var activeSpec = new ActiveProductsSpecification();
var categorySpec = new ProductsByCategorySpecification("Electronics");
var priceSpec = new ProductsByPriceRangeSpecification(100m, 200m);

var combined = activeSpec
    .And(categorySpec)
    .And(priceSpec);
// Result: Active AND Electronics AND Price between 100-200
```

Complex nested combinations:

```csharp
var activeSpec = new ActiveProductsSpecification();
var electronicsSpec = new ProductsByCategorySpecification("Electronics");
var furnitureSpec = new ProductsByCategorySpecification("Furniture");
var priceSpec = new ProductsByPriceRangeSpecification(1000m, 2000m);

// (Active AND Electronics) OR (Furniture AND NOT PriceRange)
var combined = activeSpec
    .And(electronicsSpec)
    .Or(furnitureSpec.And(priceSpec.Not()));
```

### AllOf and AnyOf Helpers

Combine multiple specifications at once:

```csharp
// AllOf: Combine with AND
var all = SpecificationExtensions.AllOf(
    new ActiveProductsSpecification(),
    new ProductsByCategorySpecification("Electronics"),
    new ProductsByPriceRangeSpecification(100m, 200m)
);
// Result: All three conditions must be satisfied

// AnyOf: Combine with OR
var any = SpecificationExtensions.AnyOf(
    new ProductsByCategorySpecification("Electronics"),
    new ProductsByCategorySpecification("Furniture"),
    new ProductsByPriceRangeSpecification(1000m, 2000m)
);
// Result: Any one condition must be satisfied
```

### Combining with Expressions

Combine specifications with expressions directly:

```csharp
var activeSpec = new ActiveProductsSpecification();

// Combine spec with expression using AND
var combined = activeSpec.And(p => p.Price > 100m);

// Combine spec with expression using OR
var orCombined = activeSpec.Or(p => p.Category == "Furniture");
```

**Combination API Reference:**

| Method | Description | Example |
|--------|-------------|---------|
| `spec1.And(spec2)` | Combine two specs with AND | `activeSpec.And(categorySpec)` |
| `spec1.Or(spec2)` | Combine two specs with OR | `spec1.Or(spec2)` |
| `spec.Not()` | Negate a specification | `activeSpec.Not()` |
| `spec.And(expr)` | Combine spec with expression (AND) | `spec.And(p => p.Price > 100)` |
| `spec.Or(expr)` | Combine spec with expression (OR) | `spec.Or(p => p.Category == "X")` |
| `AllOf(specs...)` | Combine multiple specs with AND | `AllOf(spec1, spec2, spec3)` |
| `AnyOf(specs...)` | Combine multiple specs with OR | `AnyOf(spec1, spec2, spec3)` |

## Using with Repositories

Specifications work seamlessly with repository methods.

### Basic Queries

```csharp
// Find one
var product = await repository.FindOneAsync(specification);

// Find many
var products = await repository.FindAsync(specification);

// Check existence
var exists = await repository.ExistsAsync(specification);

// Count
var count = await repository.CountAsync(specification);
```

### Pagination

Use `GetPaginatedAsync()` with specifications:

```csharp
// With specification
var result = await repository.GetPaginatedAsync(
    specification: activeSpec,
    pageNumber: 1,
    pageSize: 10
);

// With specification, sorting, and pagination
var result = await repository.GetPaginatedAsync(
    specification: activeSpec,
    pageNumber: 1,
    pageSize: 10,
    orderBy: p => p.Price,
    direction: SortDirection.Ascending
);

// Access results
var items = result.Items;        // IEnumerable<T>
var totalCount = result.TotalCount;
var pageNumber = result.PageNumber;
var pageSize = result.PageSize;
```

### Sorting

Sorting is handled at the repository level, not in specifications:

```csharp
var result = await repository.GetPaginatedAsync(
    specification: activeSpec,
    pageNumber: 1,
    pageSize: 10,
    orderBy: p => p.Name,              // Sort by name
    direction: SortDirection.Ascending  // or Descending
);
```

### Counting and Existence Checks

```csharp
// Count matching entities
var count = await repository.CountAsync(specification);

// Check if any match
var exists = await repository.ExistsAsync(specification);
```

**Repository Methods Reference:**

| Method | Description | Returns |
|--------|-------------|---------|
| `FindOneAsync(spec)` | Find first matching entity | `Task<T?>` |
| `FindAsync(spec)` | Find all matching entities | `Task<IEnumerable<T>>` |
| `ExistsAsync(spec)` | Check if any match | `Task<bool>` |
| `CountAsync(spec)` | Count matching entities | `Task<int>` |
| `GetPaginatedAsync(spec, page, size, orderBy?, direction?)` | Get paginated results | `Task<PaginatedResult<T>>` |

## In-Memory Validation

Use `IsSatisfiedBy()` to validate entities in memory (useful for domain logic and unit tests):

```csharp
var spec = new ActiveProductsSpecification();
var product = new Product("Laptop", 999m, isActive: true);

// Check if product satisfies the specification
if (spec.IsSatisfiedBy(product))
{
    // Product is active
}
```

**Example: Domain Validation**

```csharp
public class Order
{
    public bool CanBeCancelled()
    {
        var spec = new OrderCanBeCancelledSpecification();
        return spec.IsSatisfiedBy(this);
    }
}

public class OrderCanBeCancelledSpecification : BaseSpecification<Order>
{
    public OrderCanBeCancelledSpecification()
    {
        Where(o => o.Status == OrderStatus.Pending)
            .And(o => o.CreatedAt > DateTime.UtcNow.AddDays(-7));
    }
}
```

## Complete Examples

### Example 1: E-Commerce Product Search

```csharp
// Build complex search criteria
var searchSpec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Where(b => b
        .Group(g => g
            .And(p => p.Category == searchCategory)
            .And(p => p.Price >= minPrice && p.Price <= maxPrice))
        .Or(p => p.Name.Contains(searchTerm)))
    .Build();

// Get paginated results with sorting
var result = await productRepository.GetPaginatedAsync(
    specification: searchSpec,
    pageNumber: page,
    pageSize: pageSize,
    orderBy: p => p.Price,
    direction: SortDirection.Ascending
);
```

### Example 2: Combining Named Specifications

```csharp
// Define reusable specifications
var activeSpec = new ActiveProductsSpecification();
var electronicsSpec = new ProductsByCategorySpecification("Electronics");
var priceSpec = new ProductsByPriceRangeSpecification(100m, 500m);

// Combine them
var combined = activeSpec
    .And(electronicsSpec)
    .And(priceSpec);

// Use with repository
var products = await repository.FindAsync(combined);
```

### Example 3: Complex Business Rule

```csharp
// "Active products in Electronics or Furniture category, 
//  priced between 100-500, excluding discontinued items"

var activeSpec = new ActiveProductsSpecification();
var electronicsSpec = new ProductsByCategorySpecification("Electronics");
var furnitureSpec = new ProductsByCategorySpecification("Furniture");
var priceSpec = new ProductsByPriceRangeSpecification(100m, 500m);
var discontinuedSpec = new DiscontinuedProductsSpecification();

var complexSpec = activeSpec
    .And(electronicsSpec.Or(furnitureSpec))
    .And(priceSpec)
    .And(discontinuedSpec.Not());

var results = await repository.FindAsync(complexSpec);
```

### Example 4: Using AllOf Helper

```csharp
// Combine multiple specifications at once
var allConditions = SpecificationExtensions.AllOf(
    new ActiveProductsSpecification(),
    new ProductsByCategorySpecification("Electronics"),
    new ProductsByPriceRangeSpecification(100m, 500m),
    new InStockProductsSpecification()
);

var products = await repository.FindAsync(allConditions);
```

### Example 5: In-Memory Domain Validation

```csharp
public class OrderService
{
    public bool CanProcessOrder(Order order)
    {
        var spec = new OrderCanBeProcessedSpecification();
        return spec.IsSatisfiedBy(order);
    }
}

public class OrderCanBeProcessedSpecification : BaseSpecification<Order>
{
    public OrderCanBeProcessedSpecification()
    {
        Where(o => o.Status == OrderStatus.Pending)
            .And(o => o.Items.Any())
            .And(o => o.TotalAmount > 0)
            .Not(o => o.IsCancelled);
    }
}
```

## Best Practices

### 1. Use Named Specifications for Business Rules

For important domain concepts, create named specification classes:

```csharp
// ✅ Good: Named, reusable, expresses domain concept
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}

// ❌ Avoid: Anonymous, not reusable
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Build();
```

### 2. Keep Specifications Focused

Each specification should represent a single, clear business rule:

```csharp
// ✅ Good: Single responsibility
public class ActiveProductsSpecification : BaseSpecification<Product> { }
public class ProductsByCategorySpecification : BaseSpecification<Product> { }

// ❌ Avoid: Too many responsibilities
public class ActiveElectronicsProductsUnder500Specification : BaseSpecification<Product> { }
// Instead, combine: activeSpec.And(categorySpec).And(priceSpec)
```

### 3. Use FluentSpecificationBuilder for Dynamic Queries

For queries that change based on user input or runtime conditions:

```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(
    string? category, 
    decimal? minPrice, 
    decimal? maxPrice)
{
    var builder = new FluentSpecificationBuilder<Product>()
        .Where(p => p.IsActive);
    
    if (!string.IsNullOrEmpty(category))
        builder = builder.Where(p => p.Category == category);
    
    if (minPrice.HasValue)
        builder = builder.Where(p => p.Price >= minPrice.Value);
    
    if (maxPrice.HasValue)
        builder = builder.Where(p => p.Price <= maxPrice.Value);
    
    return await repository.FindAsync(builder.Build());
}
```

### 4. Combine Specifications for Complex Rules

Build complex rules by combining simple specifications:

```csharp
// ✅ Good: Composable, readable
var spec = activeSpec
    .And(categorySpec)
    .And(priceSpec);

// ❌ Avoid: One giant specification with all logic
```

### 5. Handle Null Criteria

Specifications can have null criteria (meaning "all entities"):

```csharp
var spec = new FluentSpecificationBuilder<Product>().Build();
// spec.Criteria is null - matches all products

// Always check before using
if (spec.Criteria != null)
{
    var results = await repository.FindAsync(spec);
}
```

### 6. Use IsSatisfiedBy for Domain Logic

Use `IsSatisfiedBy()` for in-memory validation in domain services:

```csharp
public class OrderService
{
    public bool CanCancelOrder(Order order)
    {
        var spec = new OrderCanBeCancelledSpecification();
        return spec.IsSatisfiedBy(order);
    }
}
```

### 7. Specifications are Immutable

All combination operations return new specifications:

```csharp
var spec1 = new ActiveProductsSpecification();
var spec2 = spec1.And(categorySpec); // spec1 is unchanged
var spec3 = spec1.Or(priceSpec);     // spec1 is still unchanged
```

### 8. Prefer Expressions Over Strings

Always use strongly-typed expressions:

```csharp
// ✅ Good: Type-safe, compile-time checked
.Where(p => p.Category == "Electronics")

// ❌ Avoid: Runtime errors, no IntelliSense
.Where("Category == 'Electronics'")
```

## Summary

The Specification pattern provides:

- **Clean API**: Fluent builder for ad-hoc specs, base class for named specs
- **Composability**: Combine specs with AND, OR, NOT operations
- **Immutability**: All operations return new specifications
- **Type Safety**: Compile-time checking of all expressions
- **Testability**: Use `IsSatisfiedBy()` for in-memory validation
- **Repository Integration**: Works seamlessly with all repository methods
- **DDD Alignment**: Pure domain logic, no infrastructure concerns

For more examples, see the test files:
- `SpecificationIntegrationTests.cs` - Basic usage and repository integration
- `SpecificationCombinationTests.cs` - Combining specifications
