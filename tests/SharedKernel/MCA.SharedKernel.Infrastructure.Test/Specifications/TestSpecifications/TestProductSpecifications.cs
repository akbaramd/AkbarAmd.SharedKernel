using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

/// <summary>
/// Basic specification for active products.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsSpecification()
    {
        Where(p => p.IsActive);
    }
}

/// <summary>
/// Specification for products by category.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ProductsByCategorySpecification : BaseSpecification<TestProduct>
{
    public ProductsByCategorySpecification(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        Where(p => p.Category == category);
    }
}

/// <summary>
/// Specification for products with price range.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ProductsByPriceRangeSpecification : BaseSpecification<TestProduct>
{
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice), "Min price cannot be negative.");
        if (maxPrice < minPrice)
            throw new ArgumentException("Max price must be greater than or equal to min price.", nameof(maxPrice));

        Where(p => p.Price >= minPrice && p.Price <= maxPrice);
    }
}

/// <summary>
/// Specification combining multiple criteria.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsByCategoryAndPriceSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsByCategoryAndPriceSpecification(string category, decimal minPrice, decimal maxPrice)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));
        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice), "Min price cannot be negative.");
        if (maxPrice < minPrice)
            throw new ArgumentException("Max price must be greater than or equal to min price.", nameof(maxPrice));

        Where(p => p.IsActive)
            .And(p => p.Category == category)
            .And(p => p.Price >= minPrice && p.Price <= maxPrice);
    }
}

/// <summary>
/// Specification demonstrating OR criteria - products in one category OR another.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ProductsByCategoryOrSpecification : BaseSpecification<TestProduct>
{
    public ProductsByCategoryOrSpecification(string category1, string category2)
    {
        if (string.IsNullOrWhiteSpace(category1))
            throw new ArgumentException("Category1 cannot be null or empty.", nameof(category1));
        if (string.IsNullOrWhiteSpace(category2))
            throw new ArgumentException("Category2 cannot be null or empty.", nameof(category2));

        // OR logic using Fluent API
        Where(p => p.Category == category1)
            .Or(p => p.Category == category2);
    }
}

/// <summary>
/// Specification demonstrating OR criteria - products with price in range OR specific category.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ProductsByPriceRangeOrCategorySpecification : BaseSpecification<TestProduct>
{
    public ProductsByPriceRangeOrCategorySpecification(decimal minPrice, decimal maxPrice, string category)
    {
        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice), "Min price cannot be negative.");
        if (maxPrice < minPrice)
            throw new ArgumentException("Max price must be greater than or equal to min price.", nameof(maxPrice));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        // OR logic using Fluent API: price in range OR specific category
        Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .Or(p => p.Category == category);
    }
}

/// <summary>
/// Specification demonstrating complex AND with OR - active products AND (category1 OR category2).
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsByCategoryOrSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsByCategoryOrSpecification(string category1, string category2)
    {
        if (string.IsNullOrWhiteSpace(category1))
            throw new ArgumentException("Category1 cannot be null or empty.", nameof(category1));
        if (string.IsNullOrWhiteSpace(category2))
            throw new ArgumentException("Category2 cannot be null or empty.", nameof(category2));

        // AND: IsActive AND (category1 OR category2)
        Where(p => p.IsActive)
            .Group(g => g
                .Or(p => p.Category == category1)
                .Or(p => p.Category == category2));
    }
}

/// <summary>
/// Specification demonstrating multiple AND criteria - active, price range, AND category.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsByMultipleAndCriteriaSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsByMultipleAndCriteriaSpecification(string category, decimal minPrice, decimal maxPrice)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));
        if (minPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(minPrice), "Min price cannot be negative.");
        if (maxPrice < minPrice)
            throw new ArgumentException("Max price must be greater than or equal to min price.", nameof(maxPrice));

        // Multiple AND criteria
        Where(p => p.IsActive)
            .And(p => p.Category == category)
            .And(p => p.Price >= minPrice)
            .And(p => p.Price <= maxPrice);
    }
}

/// <summary>
/// Specification demonstrating complex OR with multiple conditions - (price < threshold OR category) AND active.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsByComplexOrSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsByComplexOrSpecification(decimal priceThreshold, string category)
    {
        if (priceThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(priceThreshold), "Price threshold cannot be negative.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        // AND: IsActive AND (price < threshold OR category matches)
        Where(p => p.IsActive)
            .Group(g => g
                .Or(p => p.Price < priceThreshold)
                .Or(p => p.Category == category));
    }
}

#region Fluent API Specifications

/// <summary>
/// Specification using Fluent API with AND conditions.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentAndSpecification : BaseSpecification<TestProduct>
{
    public FluentAndSpecification()
    {
        Where(p => p.IsActive)
            .And(p => p.Price > 50m);
    }
}

/// <summary>
/// Specification using Fluent API with OR conditions.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentOrSpecification : BaseSpecification<TestProduct>
{
    public FluentOrSpecification()
    {
        Where(p => p.IsActive)
            .Or(p => p.Price > 200m);
    }
}

/// <summary>
/// Specification using Fluent API with NOT condition.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentNotSpecification : BaseSpecification<TestProduct>
{
    public FluentNotSpecification()
    {
        Where(p => !p.IsActive);
    }
}

/// <summary>
/// Specification using Fluent API with grouped conditions: (A AND B) OR (C AND D).
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentGroupedSpecification : BaseSpecification<TestProduct>
{
    public FluentGroupedSpecification()
    {
        Where(p => p.IsActive)
            .Group(g => g
                .And(p => p.IsActive)
                .And(p => p.Price > 50m))
            .OrGroup(g => g
                .And(p => p.Category == "Electronics")
                .And(p => p.Price < 200m));
    }
}

/// <summary>
/// Specification using Fluent API with complex nested groups.
/// Logic: ((IsActive AND Price > 50) OR (Category == "Electronics" AND NOT IsActive)) AND Price < 500
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentComplexNestedSpecification : BaseSpecification<TestProduct>
{
    public FluentComplexNestedSpecification()
    {
        Where(b => b
            .Group(g => g
                .Group(inner => inner
                    .And(p => p.IsActive)
                    .And(p => p.Price > 50m))
                .OrGroup(inner => inner
                    .And(p => p.Category == "Electronics")
                    .Not(p => p.IsActive)))
            .And(p => p.Price < 500m));
    }
}

/// <summary>
/// Specification combining multiple Where calls.
/// Each Where is combined with AND.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentAndLegacyCombinedSpecification : BaseSpecification<TestProduct>
{
    public FluentAndLegacyCombinedSpecification()
    {
        Where(p => p.Category == "Electronics");
        Where(p => p.IsActive)
            .Or(p => p.Price > 200m);
    }
}

#endregion
