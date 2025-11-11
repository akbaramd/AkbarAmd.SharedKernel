# Fluent API

The Fluent API provides a clean, chainable syntax for building specifications without creating custom specification classes.

## Overview

`FluentSpecificationBuilder<T>` allows you to build complete specifications inline:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Page(1, 10)
    .Build();
```

## Method Chaining

All methods return `FluentSpecificationBuilder<T>`, enabling method chaining:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)           // Returns FluentSpecificationBuilder<Product>
    .Include(p => p.Category)          // Returns FluentSpecificationBuilder<Product>
    .OrderBy(p => p.Name)              // Returns FluentSpecificationBuilder<Product>
    .Page(1, 10)                       // Returns FluentSpecificationBuilder<Product>
    .Build();                          // Returns ISpecification<Product>
```

## Where Clauses

### Simple Where

```csharp
.Where(p => p.IsActive)
```

### Complex Where with Builder

```csharp
.Where(b => b
    .Group(g => g
        .And(p => p.IsActive)
        .And(p => p.Price > 100))
    .Or(p => p.IsFeatured))
```

## Sorting Methods

### OrderBy

```csharp
.OrderBy(p => p.Name)
.OrderBy(p => p.CreatedDate)
```

### OrderByDescending

```csharp
.OrderByDescending(p => p.Price)
.OrderByDescending(p => p.UpdatedDate)
```

### ThenBy (Secondary Sort)

```csharp
.OrderBy(p => p.Category)
    .ThenBy(p => p.Name)
    .ThenByDescending(p => p.Price)
```

### Null Handling

```csharp
.OrderBy(p => p.ExpiryDate)
    .NullsFirst()  // or .NullsLast()
```

## Pagination Methods

### Page

```csharp
.Page(pageNumber: 1, pageSize: 10)
```

### Skip and Take

```csharp
.Skip(20)
.Take(10)
```

## Include Methods

### Single Include

```csharp
.Include(p => p.Category)
```

### Multiple Includes

```csharp
.Include(p => p.Category, p => p.Reviews, p => p.Manufacturer)
```

### String-based Includes

```csharp
.Include("Category")
.IncludePaths("Reviews", "Reviews.Reviewer")
```

## Complete Example

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    // Criteria
    .Where(b => b
        .Group(g => g
            .And(p => p.IsActive)
            .And(p => p.Price >= 50)
            .And(p => p.Price <= 500))
        .Or(p => p.IsFeatured))
    
    // Includes
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .IncludePaths("Manufacturer", "Manufacturer.Address")
    
    // Sorting
    .OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .ThenBy(p => p.Name)
    
    // Pagination
    .Page(1, 20)
    
    // Build
    .Build();

var result = await repository.GetPaginatedAsync(1, 20, spec);
```

## When to Use Fluent API

**Use Fluent API when:**
- Building ad-hoc queries
- Query logic is specific to one use case
- Need dynamic query building
- Prototyping or quick queries

**Use BaseSpecification when:**
- Query logic is reusable
- Need to encapsulate business rules
- Want to name and document the specification
- Following DDD patterns

