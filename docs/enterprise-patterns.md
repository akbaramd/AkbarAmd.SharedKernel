# Enterprise Patterns

## Specification Composition

### Combining Specifications

```csharp
public class ProductSpecifications
{
    public static ISpecification<Product> Active() =>
        new FluentSpecificationBuilder<Product>()
            .Where(p => p.IsActive)
            .Build();

    public static ISpecification<Product> InCategory(string category) =>
        new FluentSpecificationBuilder<Product>()
            .Where(p => p.Category == category)
            .Build();

    public static ISpecification<Product> PriceRange(decimal min, decimal max) =>
        new FluentSpecificationBuilder<Product>()
            .Where(p => p.Price >= min && p.Price <= max)
            .Build();
}

// Usage: Combine specifications
var spec = SpecificationExtensions
    .And(ProductSpecifications.Active(), 
         ProductSpecifications.InCategory("Electronics"),
         ProductSpecifications.PriceRange(100, 500));
```

### Reusable Specification Classes

```csharp
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}

public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category)
    {
        Where(p => p.Category == category);
    }
}

public class FeaturedProductsSpecification : BaseSpecification<Product>
{
    public FeaturedProductsSpecification()
    {
        Where(p => p.IsFeatured);
        OrderByDescending(p => p.FeaturedDate);
    }
}

// Usage
var activeElectronics = new ActiveProductsSpecification()
    .And(new ProductsByCategorySpecification("Electronics"));
```

## Domain-Driven Design (DDD) Patterns

### Specification as Business Rules

```csharp
public class OrderCanBeShippedSpecification : BaseSpecification<Order>
{
    public OrderCanBeShippedSpecification()
    {
        Where(o => o.Status == OrderStatus.Confirmed);
        Where(o => o.PaymentStatus == PaymentStatus.Paid);
        Where(o => o.ShippingAddress != null);
        Where(o => !o.Items.Any(i => i.Product.IsOutOfStock));
    }
}

// Usage in domain service
public class OrderService
{
    public async Task<bool> CanShipOrderAsync(Order order)
    {
        var spec = new OrderCanBeShippedSpecification();
        return await _orderRepository.ExistsAsync(spec);
    }
}
```

### Value Object Specifications

```csharp
public class HighValueOrderSpecification : BaseSpecification<Order>
{
    private readonly Money _threshold;

    public HighValueOrderSpecification(Money threshold)
    {
        _threshold = threshold;
        Where(o => o.TotalAmount >= _threshold);
    }
}
```

## CQRS Integration

### Query Specifications

```csharp
// Query
public class GetProductsQuery
{
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Query Handler
public class GetProductsQueryHandler
{
    private readonly IProductRepository _repository;

    public async Task<PaginatedResult<ProductDto>> HandleAsync(GetProductsQuery query)
    {
        var builder = new FluentSpecificationBuilder<Product>();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            builder.Where(p => p.Category == query.Category);
        }

        if (query.MinPrice.HasValue)
        {
            builder.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            builder.Where(p => p.Price <= query.MaxPrice.Value);
        }

        var spec = builder
            .OrderBy(p => p.Name)
            .Page(query.Page, query.PageSize)
            .Build();

        var result = await _repository.GetPaginatedAsync(query.Page, query.PageSize, spec);
        return MapToDto(result);
    }
}
```

## Service Layer Patterns

### Specification Factory

```csharp
public interface ISpecificationFactory
{
    ISpecification<Product> CreateProductSpecification(ProductFilter filter);
}

public class SpecificationFactory : ISpecificationFactory
{
    public ISpecification<Product> CreateProductSpecification(ProductFilter filter)
    {
        var builder = new FluentSpecificationBuilder<Product>();

        if (filter.IsActive.HasValue)
        {
            builder.Where(p => p.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            builder.Where(p => p.Category == filter.Category);
        }

        if (filter.MinPrice.HasValue)
        {
            builder.Where(p => p.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            builder.Where(p => p.Price <= filter.MaxPrice.Value);
        }

        // Sorting
        switch (filter.SortBy)
        {
            case "name":
                builder.OrderBy(p => p.Name);
                break;
            case "price":
                builder.OrderByDescending(p => p.Price);
                break;
            default:
                builder.OrderBy(p => p.CreatedDate);
                break;
        }

        return builder.Build();
    }
}
```

## Best Practices

### 1. Naming Conventions

```csharp
// Good: Clear, descriptive names
public class ActiveProductsSpecification : BaseSpecification<Product> { }
public class ProductsByCategorySpecification : BaseSpecification<Product> { }
public class HighValueOrdersSpecification : BaseSpecification<Order> { }

// Bad: Vague names
public class ProductSpec1 : BaseSpecification<Product> { }
public class Spec : BaseSpecification<Product> { }
```

### 2. Single Responsibility

```csharp
// Good: Each specification has one clear purpose
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification() => Where(p => p.IsActive);
}

// Bad: Multiple responsibilities
public class ProductSpecification : BaseSpecification<Product>
{
    public ProductSpecification(bool active, string category, decimal price)
    {
        // Too many responsibilities
    }
}
```

### 3. Immutability

```csharp
// Specifications should be immutable after construction
var spec = new ActiveProductsSpecification();
// spec cannot be modified - good for thread safety
```

### 4. Testability

```csharp
[Fact]
public async Task ActiveProductsSpecification_ReturnsOnlyActiveProducts()
{
    // Arrange
    var spec = new ActiveProductsSpecification();
    
    // Act
    var results = await _repository.FindAsync(spec);
    
    // Assert
    Assert.All(results, p => Assert.True(p.IsActive));
}
```

### 5. Documentation

```csharp
/// <summary>
/// Specification for finding active products.
/// Matches products where IsActive is true.
/// </summary>
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}
```

## Error Handling

### Validation

```csharp
public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty", nameof(category));

        Where(p => p.Category == category);
    }
}
```

### Null Safety

```csharp
var builder = new FluentSpecificationBuilder<Product>();

if (filter?.Category != null)
{
    builder.Where(p => p.Category == filter.Category);
}

var spec = builder.Build();
```

