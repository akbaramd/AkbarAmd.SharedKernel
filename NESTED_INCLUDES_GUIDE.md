# Nested Includes with ThenInclude - Implementation Guide

## Overview

The specification pattern now supports nested includes using `Include().ThenInclude()` chains, with full support for nullable navigation properties at any level.

## Key Features

1. **Nested Includes**: Support for Include -> ThenInclude -> ThenInclude... chains
2. **Nullable Navigation Properties**: Automatically handles nullable navigation properties at any level
3. **Fluent API**: Clean, chainable API for building include paths
4. **Backward Compatible**: Existing simple includes continue to work

## Usage Examples

### Basic Nested Include

```csharp
// In a specification class
public sealed class ProductsWithReviewsAndProductSpecification : BaseSpecification<TestProduct>
{
    public ProductsWithReviewsAndProductSpecification()
    {
        IncludeChain(p => p.Reviews)
            .ThenInclude<TestProductReview, TestProduct>(r => r.Product)
            .Build();
    }
}
```

### Multiple Nested Levels

```csharp
// Include -> ThenInclude -> ThenInclude
public sealed class ComplexIncludeSpecification : BaseSpecification<Order>
{
    public ComplexIncludeSpecification()
    {
        IncludeChain(o => o.OrderItems)
            .ThenInclude<OrderItem, Product>(item => item.Product)
            .ThenInclude<Product, Category>(p => p.Category)
            .Build();
    }
}
```

### Nullable Navigation Properties

The system automatically handles nullable navigation properties:

```csharp
// If Product is nullable in OrderItem
public sealed class OrderWithNullableProductSpecification : BaseSpecification<Order>
{
    public OrderWithNullableProductSpecification()
    {
        // This works even if Product is nullable
        IncludeChain(o => o.OrderItems)
            .ThenInclude<OrderItem, Product?>(item => item.Product)  // Nullable type
            .ThenInclude<Product, Category>(p => p.Category)
            .Build();
    }
}
```

### Using FluentSpecificationBuilder

```csharp
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .IncludeChain(p => p.Reviews)
        .ThenInclude<Review, User>(r => r.Reviewer)
        .ThenInclude<User, Profile>(u => u.Profile)
        .Build()
    .OrderByKey(p => p.Name)
    .Build();
```

### Combining Simple and Nested Includes

You can mix simple includes with nested include chains:

```csharp
public sealed class MixedIncludesSpecification : BaseSpecification<Product>
{
    public MixedIncludesSpecification()
    {
        // Simple include
        AddInclude(p => p.Category);
        
        // Nested include chain
        IncludeChain(p => p.Reviews)
            .ThenInclude<Review, User>(r => r.Reviewer)
            .Build();
    }
}
```

## How It Works

### 1. IncludeChain Structure

The `IncludeChain<T>` class stores:
- The initial `Include` expression
- A list of `ThenInclude` expressions in order

### 2. Type Resolution

The EF Core evaluator automatically:
- Extracts navigation property types from expressions
- Handles nullable types by unwrapping `Nullable<T>`
- Finds the correct `ThenInclude` method overload using reflection
- Applies includes in the correct order

### 3. Nullable Support

Nullable navigation properties are handled automatically:
- The system unwraps `Nullable<T>` to get the underlying type
- EF Core's `ThenInclude` methods work correctly with nullable properties
- No special syntax needed - just use the nullable type in your expression

## Implementation Details

### IncludeChain Class

```csharp
public sealed class IncludeChain<T>
{
    public Expression<Func<T, object>> IncludeExpression { get; }
    public IReadOnlyList<LambdaExpression> ThenIncludeExpressions { get; }
    
    public IncludeChain<T> ThenInclude<TPrevious, TProperty>(
        Expression<Func<TPrevious, TProperty>> thenIncludeExpression);
}
```

### IncludeChainBuilder

The `IncludeChainBuilder<T>` provides the fluent API:

```csharp
public sealed class IncludeChainBuilder<T>
{
    public IncludeChainBuilder<T> ThenInclude<TPrevious, TProperty>(
        Expression<Func<TPrevious, TProperty>> thenIncludeExpression);
    
    public BaseSpecification<T> Build();
}
```

### EF Core Evaluator

The `EfCoreSpecificationEvaluator` applies include chains:

1. Applies the initial `Include` using the `IncludeExpression`
2. For each `ThenInclude` expression:
   - Extracts the navigation property type
   - Handles nullable types
   - Finds the correct `ThenInclude` method
   - Applies it to the query

## Best Practices

1. **Use IncludeChain for nested includes**: Use `IncludeChain()` when you need `ThenInclude`
2. **Use simple Include for single-level**: Use `AddInclude()` or `Include()` for single-level includes
3. **Specify nullable types explicitly**: If a navigation property is nullable, use the nullable type in the expression
4. **Chain ThenInclude calls**: Build the entire chain before calling `Build()`

## Migration from Simple Includes

If you have existing specifications with simple includes that you want to convert to nested includes:

**Before:**
```csharp
AddInclude(p => p.Reviews);
```

**After (if you need nested includes):**
```csharp
IncludeChain(p => p.Reviews)
    .ThenInclude<Review, User>(r => r.Reviewer)
    .Build();
```

**Note**: You can keep simple includes if you don't need nested includes - they still work!

## Error Handling

The system will throw `InvalidOperationException` if:
- It cannot find an appropriate `ThenInclude` method for the type combination
- The include chain is malformed

Common causes:
- Type mismatch between ThenInclude calls
- Missing navigation properties
- Incorrect generic type parameters

## Testing

Example test specification:

```csharp
public sealed class TestNestedIncludeSpecification : BaseSpecification<TestProduct>
{
    public TestNestedIncludeSpecification()
    {
        Where(p => p.IsActive);
        IncludeChain(p => p.Reviews)
            .ThenInclude<TestProductReview, TestProduct>(r => r.Product)
            .Build();
    }
}
```

## Summary

The nested includes feature provides:
- ✅ Full support for `Include().ThenInclude()` chains
- ✅ Automatic handling of nullable navigation properties
- ✅ Clean, fluent API
- ✅ Backward compatibility with existing code
- ✅ Type-safe include chains

Use `IncludeChain()` when you need nested includes, and the system will handle the rest!

