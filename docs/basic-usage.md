# Basic Usage

## Criteria Building

### Simple Criteria

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Build();
```

### Multiple AND Conditions

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Where(p => p.Price > 100)
    .Where(p => p.Category == "Electronics")
    .Build();
```

### OR Conditions

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(b => b
        .Group(g => g
            .And(p => p.Category == "Electronics")
            .And(p => p.Price > 100))
        .Or(p => p.IsFeatured))
    .Build();
```

### Complex Grouping

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(b => b
        .Group(g => g
            .And(p => p.IsActive)
            .And(p => p.Price > 50))
        .OrGroup(g => g
            .And(p => p.Category == "Electronics")
            .And(p => p.Price < 200)))
    .Build();
```

## Sorting

### Single Sort

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Build();
```

### Descending Sort

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderByDescending(p => p.Price)
    .Build();
```

### Multi-level Sorting

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .ThenBy(p => p.Name)
    .Build();
```

### Null Handling

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.ExpiryDate)
    .NullsLast()  // or .NullsFirst()
    .Build();
```

## Pagination

### Page-based Pagination

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Page(pageNumber: 1, pageSize: 10)
    .Build();

var result = await repository.GetPaginatedAsync(1, 10, spec);
// result.PageNumber, result.PageSize, result.TotalCount, result.Items
```

### Skip/Take Pagination

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Skip(20)
    .Take(10)
    .Build();
```

## Includes (Eager Loading)

### Single Include

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Include(p => p.Category)
    .Build();
```

### Multiple Includes

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .Include(p => p.Manufacturer)
    .Build();
```

### String-based Includes

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Include("Category")
    .Include("Reviews.Reviewer")
    .Build();
```

## Complete Example

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(b => b
        .Group(g => g
            .And(p => p.IsActive)
            .And(p => p.Price > 50))
        .Or(p => p.IsFeatured))
    .Include(p => p.Category)
    .Include(p => p.Reviews)
    .OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .Page(1, 20)
    .Build();

var result = await repository.GetPaginatedAsync(1, 20, spec);
```

