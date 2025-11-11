# API Reference

## FluentSpecificationBuilder<T>

Fluent builder for creating ad-hoc specifications.

### Methods

#### Where

```csharp
FluentSpecificationBuilder<T> Where(Expression<Func<T, bool>> expr)
```

Adds a simple criteria expression.

**Example:**
```csharp
.Where(p => p.IsActive)
```

---

```csharp
FluentSpecificationBuilder<T> Where(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
```

Adds complex criteria using a builder function.

**Example:**
```csharp
.Where(b => b
    .Group(g => g
        .And(p => p.IsActive)
        .And(p => p.Price > 100)))
```

#### Include

```csharp
FluentSpecificationBuilder<T> Include(Expression<Func<T, object>> include)
```

Adds an include expression for eager loading.

**Example:**
```csharp
.Include(p => p.Category)
```

---

```csharp
FluentSpecificationBuilder<T> Include(string path)
```

Adds an include by string path.

**Example:**
```csharp
.Include("Category")
```

---

```csharp
FluentSpecificationBuilder<T> Include(params Expression<Func<T, object>>[] includes)
```

Adds multiple include expressions.

**Example:**
```csharp
.Include(p => p.Category, p => p.Reviews)
```

---

```csharp
FluentSpecificationBuilder<T> IncludePaths(params string[] paths)
```

Adds multiple include paths.

**Example:**
```csharp
.IncludePaths("Category", "Reviews", "Reviews.Reviewer")
```

#### OrderBy

```csharp
FluentSpecificationBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> key)
```

Sets the primary sort key in ascending order.

**Example:**
```csharp
.OrderBy(p => p.Name)
```

---

```csharp
FluentSpecificationBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> key)
```

Sets the primary sort key in descending order.

**Example:**
```csharp
.OrderByDescending(p => p.Price)
```

#### ThenBy

```csharp
FluentSpecificationBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> key)
```

Adds a secondary sort key in ascending order.

**Example:**
```csharp
.OrderBy(p => p.Category)
    .ThenBy(p => p.Name)
```

---

```csharp
FluentSpecificationBuilder<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key)
```

Adds a secondary sort key in descending order.

**Example:**
```csharp
.OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
```

#### Null Handling

```csharp
FluentSpecificationBuilder<T> NullsFirst()
```

Sets null ordering policy to NullsFirst for the last sort descriptor.

**Example:**
```csharp
.OrderBy(p => p.ExpiryDate)
    .NullsFirst()
```

---

```csharp
FluentSpecificationBuilder<T> NullsLast()
```

Sets null ordering policy to NullsLast for the last sort descriptor.

**Example:**
```csharp
.OrderBy(p => p.ExpiryDate)
    .NullsLast()
```

#### Pagination

```csharp
FluentSpecificationBuilder<T> Page(int page, int size)
```

Sets pagination using page number and page size.

**Example:**
```csharp
.Page(1, 20)
```

---

```csharp
FluentSpecificationBuilder<T> Skip(int skip)
```

Sets the skip value for pagination.

**Example:**
```csharp
.Skip(20)
```

---

```csharp
FluentSpecificationBuilder<T> Take(int take)
```

Sets the take value for pagination.

**Example:**
```csharp
.Take(10)
```

#### Build

```csharp
ISpecification<T> Build()
```

Builds and returns the final specification.

**Example:**
```csharp
var spec = builder.Build();
```

## BaseSpecification<T>

Abstract base class for creating reusable specifications.

### Protected Methods

#### Where

```csharp
protected ICriteriaChain<T> Where(Expression<Func<T, bool>> start)
```

Entry point for fluent API: Starts a criteria chain with an initial expression.

**Example:**
```csharp
public class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}
```

---

```csharp
protected ICriteriaChain<T> Where(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
```

Entry point for fluent API: Starts a criteria chain using a builder function.

**Example:**
```csharp
Where(b => b
    .Group(g => g
        .And(p => p.IsActive)
        .And(p => p.Price > 100)))
```

#### AddCriteria

```csharp
protected void AddCriteria(Expression<Func<T, bool>> criteriaExpression)
```

Legacy API: Adds a criteria expression that will be combined with AND.

**Example:**
```csharp
AddCriteria(p => p.IsActive);
AddCriteria(p => p.Price > 100);
// Results in: p.IsActive && p.Price > 100
```

### Public Properties

```csharp
Expression<Func<T, bool>>? Criteria { get; }
```

The combined criteria expression.

---

```csharp
IReadOnlyList<Expression<Func<T, object>>> Includes { get; }
```

List of include expressions.

---

```csharp
IReadOnlyList<string> IncludeStrings { get; }
```

List of include string paths.

---

```csharp
IReadOnlyList<SortDescriptor<T>> Sorts { get; }
```

List of sort descriptors for multi-level sorting.

---

```csharp
int Take { get; }
```

Number of items to take for pagination.

---

```csharp
int Skip { get; }
```

Number of items to skip for pagination.

---

```csharp
bool IsPagingEnabled { get; }
```

Indicates if pagination is enabled.

## ICriteriaChain<T>

Interface for building complex criteria with AND, OR, NOT operations.

### Methods

#### And

```csharp
ICriteriaChain<T> And(Expression<Func<T, bool>> expr)
```

Adds an AND condition.

#### Or

```csharp
ICriteriaChain<T> Or(Expression<Func<T, bool>> expr)
```

Adds an OR condition.

#### Not

```csharp
ICriteriaChain<T> Not(Expression<Func<T, bool>> expr)
```

Adds a NOT condition.

#### Group

```csharp
ICriteriaChain<T> Group(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
```

Creates a grouped AND condition.

#### OrGroup

```csharp
ICriteriaChain<T> OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
```

Creates a grouped OR condition.

