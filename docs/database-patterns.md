# Database Patterns

## Repository Pattern Integration

The Specification Pattern integrates seamlessly with the Repository Pattern:

```csharp
public interface IProductRepository
{
    Task<IEnumerable<Product>> FindAsync(ISpecification<Product> specification);
    Task<Product?> FindOneAsync(ISpecification<Product> specification);
    Task<int> CountAsync(ISpecification<Product> specification);
    Task<bool> ExistsAsync(ISpecification<Product> specification);
    Task<PaginatedResult<Product>> GetPaginatedAsync(IPaginatedSpecification<Product> specification);
}
```

### Usage Example

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<Product>> GetActiveProductsAsync(int page, int size)
    {
        var spec = new FluentSpecificationBuilder<Product>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Page(page, size)
            .Build();

        return await _repository.GetPaginatedAsync(page, size, spec);
    }
}
```

## Entity Framework Core Integration

### Automatic Query Translation

Specifications are automatically translated to efficient SQL queries:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive && p.Price > 100)
    .Include(p => p.Category)
    .OrderBy(p => p.Name)
    .Page(1, 10)
    .Build();

// Translated to:
// SELECT p.*, c.*
// FROM Products p
// LEFT JOIN Categories c ON p.CategoryId = c.Id
// WHERE p.IsActive = 1 AND p.Price > 100
// ORDER BY p.Name
// OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
```

### Performance Optimization

#### AsNoTracking

For read-only queries, use `AsNoTracking`:

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Build();

// In repository implementation
var query = _dbContext.Products
    .AsNoTracking()  // Improves performance for read-only queries
    .ApplySpecification(spec);
```

#### Split Queries

For complex includes, use split queries:

```csharp
// In EfCoreSpecificationEvaluator options
var options = new SpecificationEvaluationOptions
{
    UseSplitQuery = true  // Prevents cartesian explosion
};
```

## Common Database Patterns

### Pattern 1: Filtered List with Pagination

```csharp
public async Task<PaginatedResult<Product>> GetProductsByCategoryAsync(
    string category, 
    int page, 
    int size)
{
    var spec = new FluentSpecificationBuilder<Product>()
        .Where(p => p.Category == category)
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .Page(page, size)
        .Build();

    return await _repository.GetPaginatedAsync(page, size, spec);
}
```

### Pattern 2: Search with Multiple Criteria

```csharp
public async Task<IEnumerable<Product>> SearchProductsAsync(
    string? searchTerm,
    decimal? minPrice,
    decimal? maxPrice,
    string? category)
{
    var builder = new FluentSpecificationBuilder<Product>();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        builder.Where(p => p.Name.Contains(searchTerm) || 
                          p.Description.Contains(searchTerm));
    }

    if (minPrice.HasValue)
    {
        builder.Where(p => p.Price >= minPrice.Value);
    }

    if (maxPrice.HasValue)
    {
        builder.Where(p => p.Price <= maxPrice.Value);
    }

    if (!string.IsNullOrWhiteSpace(category))
    {
        builder.Where(p => p.Category == category);
    }

    builder.OrderBy(p => p.Name);

    var spec = builder.Build();
    return await _repository.FindAsync(spec);
}
```

### Pattern 3: Complex Filtering with OR Logic

```csharp
public async Task<IEnumerable<Product>> GetFeaturedOrNewProductsAsync()
{
    var spec = new FluentSpecificationBuilder<Product>()
        .Where(b => b
            .Group(g => g
                .And(p => p.IsActive)
                .And(p => p.IsFeatured))
            .OrGroup(g => g
                .And(p => p.IsActive)
                .And(p => p.CreatedDate >= DateTime.UtcNow.AddDays(-7))))
        .OrderByDescending(p => p.CreatedDate)
        .Build();

    return await _repository.FindAsync(spec);
}
```

### Pattern 4: Count Optimization

```csharp
// Count queries automatically exclude includes for performance
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .Include(p => p.Reviews)  // Ignored in count query
    .Build();

var count = await _repository.CountAsync(spec);
// Executes: SELECT COUNT(*) FROM Products WHERE IsActive = 1
```

## Performance

### Best Practices

1. **Use AsNoTracking for Read-Only Queries**
   ```csharp
   // In repository
   return await _dbContext.Products
       .AsNoTracking()
       .ApplySpecification(spec)
       .ToListAsync();
   ```

2. **Limit Includes**
   ```csharp
   // Only include what you need
   .Include(p => p.Category)  // Good
   .Include(p => p.Reviews)    // Only if needed
   ```

3. **Use Indexed Columns for Filtering**
   ```csharp
   // Ensure database indexes exist for filtered columns
   .Where(p => p.IsActive)      // Index on IsActive
   .Where(p => p.Category)      // Index on Category
   ```

4. **Pagination for Large Datasets**
   ```csharp
   // Always use pagination for potentially large result sets
   .Page(1, 20)  // Never fetch all records
   ```

### Query Optimization

The specification evaluator automatically optimizes queries:

- **Includes are excluded from COUNT queries**
- **Includes are excluded from EXISTS queries**
- **Sorting is applied at database level**
- **Pagination uses OFFSET/FETCH for efficiency**

