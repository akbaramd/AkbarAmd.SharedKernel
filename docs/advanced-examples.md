# Advanced Examples

## Dynamic Query Building

### Building Specifications from User Input

```csharp
public class ProductSearchService
{
    public async Task<PaginatedResult<Product>> SearchAsync(ProductSearchRequest request)
    {
        var builder = new FluentSpecificationBuilder<Product>();

        // Dynamic criteria building
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            builder.Where(p => 
                p.Name.Contains(request.SearchTerm) || 
                p.Description.Contains(request.SearchTerm));
        }

        if (request.Categories?.Any() == true)
        {
            builder.Where(b => b
                .Group(g => g
                    .And(p => request.Categories.Contains(p.Category))));
        }

        if (request.MinPrice.HasValue)
        {
            builder.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            builder.Where(p => p.Price <= request.MaxPrice.Value);
        }

        if (request.IsActive.HasValue)
        {
            builder.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Dynamic sorting
        switch (request.SortBy?.ToLower())
        {
            case "name":
                builder.OrderBy(p => p.Name);
                break;
            case "price":
                builder.OrderByDescending(p => p.Price);
                break;
            case "date":
                builder.OrderByDescending(p => p.CreatedDate);
                break;
            default:
                builder.OrderBy(p => p.Name);
                break;
        }

        // Pagination
        builder.Page(request.Page, request.PageSize);

        var spec = builder.Build();
        return await _repository.GetPaginatedAsync(request.Page, request.PageSize, spec);
    }
}
```

## Complex Business Rules

### Multi-Condition Business Logic

```csharp
public class EligibleForDiscountSpecification : BaseSpecification<Order>
{
    public EligibleForDiscountSpecification()
    {
        Where(b => b
            .Group(g => g
                .And(o => o.Status == OrderStatus.Pending)
                .And(o => o.TotalAmount >= 100)
                .And(o => o.Customer.MembershipLevel == MembershipLevel.Premium))
            .OrGroup(g => g
                .And(o => o.Status == OrderStatus.Pending)
                .And(o => o.TotalAmount >= 500)
                .And(o => o.Items.Count >= 5)));
    }
}
```

### Time-Based Specifications

```csharp
public class RecentOrdersSpecification : BaseSpecification<Order>
{
    public RecentOrdersSpecification(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        Where(o => o.CreatedDate >= cutoffDate);
        OrderByDescending(o => o.CreatedDate);
    }
}

public class ExpiringProductsSpecification : BaseSpecification<Product>
{
    public ExpiringProductsSpecification(int daysUntilExpiry)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        Where(p => p.ExpiryDate.HasValue);
        Where(p => p.ExpiryDate <= expiryDate);
        Where(p => p.ExpiryDate > DateTime.UtcNow);
        OrderBy(p => p.ExpiryDate);
    }
}
```

## Nested Specifications

### Specifications with Related Entity Criteria

```csharp
public class ProductsWithHighRatedReviewsSpecification : BaseSpecification<Product>
{
    public ProductsWithHighRatedReviewsSpecification(int minRating)
    {
        Where(p => p.Reviews.Any(r => r.Rating >= minRating && r.IsApproved));
        Include(p => p.Reviews);
        OrderByDescending(p => p.Reviews.Average(r => r.Rating));
    }
}
```

## Performance-Critical Scenarios

### Optimized Count Queries

```csharp
// Count specification (excludes includes automatically)
var countSpec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Where(p => p.Category == "Electronics")
    .Build();

var totalCount = await _repository.CountAsync(countSpec);

// Then fetch with includes only if needed
if (totalCount > 0)
{
    var dataSpec = new FluentSpecificationBuilder<Product>()
        .Where(p => p.IsActive)
        .Where(p => p.Category == "Electronics")
        .Include(p => p.Category)
        .Include(p => p.Reviews)
        .Page(1, 20)
        .Build();

    var products = await _repository.GetPaginatedAsync(1, 20, dataSpec);
}
```

### Batch Processing

```csharp
public async Task ProcessProductsInBatchesAsync(int batchSize)
{
    var baseSpec = new FluentSpecificationBuilder<Product>()
        .Where(p => p.IsActive)
        .OrderBy(p => p.Id)
        .Build();

    int page = 1;
    bool hasMore = true;

    while (hasMore)
    {
        var batchSpec = new FluentSpecificationBuilder<Product>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .Page(page, batchSize)
            .Build();

        var batch = await _repository.GetPaginatedAsync(page, batchSize, batchSpec);
        
        await ProcessBatchAsync(batch.Items);

        hasMore = batch.Items.Count() == batchSize;
        page++;
    }
}
```

## Specification Composition Patterns

### Fluent Composition

```csharp
public static class ProductSpecificationExtensions
{
    public static FluentSpecificationBuilder<Product> Active(
        this FluentSpecificationBuilder<Product> builder)
    {
        return builder.Where(p => p.IsActive);
    }

    public static FluentSpecificationBuilder<Product> InCategory(
        this FluentSpecificationBuilder<Product> builder, 
        string category)
    {
        return builder.Where(p => p.Category == category);
    }

    public static FluentSpecificationBuilder<Product> PriceRange(
        this FluentSpecificationBuilder<Product> builder, 
        decimal min, 
        decimal max)
    {
        return builder.Where(p => p.Price >= min && p.Price <= max);
    }
}

// Usage
var spec = new FluentSpecificationBuilder<Product>()
    .Active()
    .InCategory("Electronics")
    .PriceRange(100, 500)
    .OrderBy(p => p.Name)
    .Page(1, 20)
    .Build();
```

## Integration with MediatR

### Query with Specification

```csharp
public class GetProductsQuery : IRequest<PaginatedResult<ProductDto>>
{
    public ProductFilter Filter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public async Task<PaginatedResult<ProductDto>> Handle(
        GetProductsQuery request, 
        CancellationToken cancellationToken)
    {
        var spec = BuildSpecification(request.Filter, request.Page, request.PageSize);
        var result = await _repository.GetPaginatedAsync(request.Page, request.PageSize, spec);
        
        return new PaginatedResult<ProductDto>
        {
            Items = _mapper.Map<IEnumerable<ProductDto>>(result.Items),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    private ISpecification<Product> BuildSpecification(
        ProductFilter filter, 
        int page, 
        int pageSize)
    {
        var builder = new FluentSpecificationBuilder<Product>();

        if (filter.IsActive.HasValue)
            builder.Where(p => p.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            builder.Where(p => p.Category == filter.Category);

        return builder
            .OrderBy(p => p.Name)
            .Page(page, pageSize)
            .Build();
    }
}
```

