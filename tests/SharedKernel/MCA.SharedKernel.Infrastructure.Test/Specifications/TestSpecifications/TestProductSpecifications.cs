using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

/// <summary>
/// Basic specification for active products.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsSpecification : Specification<TestProduct>
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
public sealed class ProductsByCategorySpecification : Specification<TestProduct>
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
public sealed class ProductsByPriceRangeSpecification : Specification<TestProduct>
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
public sealed class ActiveProductsByCategoryAndPriceSpecification : Specification<TestProduct>
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
public sealed class ProductsByCategoryOrSpecification : Specification<TestProduct>
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
public sealed class ProductsByPriceRangeOrCategorySpecification : Specification<TestProduct>
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
public sealed class ActiveProductsByCategoryOrSpecification : Specification<TestProduct>
{
    public ActiveProductsByCategoryOrSpecification(string category1, string category2)
    {
        if (string.IsNullOrWhiteSpace(category1))
            throw new ArgumentException("Category1 cannot be null or empty.", nameof(category1));
        if (string.IsNullOrWhiteSpace(category2))
            throw new ArgumentException("Category2 cannot be null or empty.", nameof(category2));

        // AND: IsActive AND (category1 OR category2)
        // LINQ Equivalent: IsActive && (Category == category1 || Category == category2)
        Where(p => p.IsActive)
            .AndGroup(g => g
                .Where(p => p.Category == category1)
                .Or(p => p.Category == category2));
    }
}

/// <summary>
/// Specification demonstrating multiple AND criteria - active, price range, AND category.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsByMultipleAndCriteriaSpecification : Specification<TestProduct>
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
public sealed class ActiveProductsByComplexOrSpecification : Specification<TestProduct>
{
    public ActiveProductsByComplexOrSpecification(decimal priceThreshold, string category)
    {
        if (priceThreshold < 0)
            throw new ArgumentOutOfRangeException(nameof(priceThreshold), "Price threshold cannot be negative.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        // AND: IsActive AND (price < threshold OR category matches)
        // LINQ Equivalent: IsActive && (Price < priceThreshold || Category == category)
        Where(p => p.IsActive)
            .AndGroup(g => g
                .Where(p => p.Price < priceThreshold)
                .Or(p => p.Category == category));
    }
}

#region Fluent API Specifications

/// <summary>
/// Specification using Fluent API with AND conditions.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentAndSpecification : Specification<TestProduct>
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
public sealed class FluentOrSpecification : Specification<TestProduct>
{
    public FluentOrSpecification()
    {
        Where(p => p.IsActive)
            .Or(p => p.Price > 200m);
    }
}

/// <summary>
/// Specification using Fluent API with NOT condition.
/// Demonstrates using Not() method to negate conditions.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentNotSpecification : Specification<TestProduct>
{
    public FluentNotSpecification()
    {
        // Example 1: Using ! operator to negate a condition
        // LINQ Equivalent: p => !p.IsActive
        Where(b => b
            .And(p => !p.IsActive));
    }
}

/// <summary>
/// Specification for products with non-zero price using Not().
/// Demonstrates: Products where Price is NOT zero.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ProductsWithNonZeroPriceSpecification : Specification<TestProduct>
{
    public ProductsWithNonZeroPriceSpecification()
    {
        // Example: Get products where Price is NOT zero
        // Using ! operator for negation
        // LINQ Equivalent: p => p.Price != 0
        Where(b => b
            .And(p => !(p.Price == 0m)));
        
        // Alternative (more readable for simple != comparisons):
        // Where(p => p.Price != 0m);
    }
}

/// <summary>
/// Specification for active products with non-zero price.
/// Demonstrates combining Where, And, and Not.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class ActiveProductsWithNonZeroPriceSpecification : Specification<TestProduct>
{
    public ActiveProductsWithNonZeroPriceSpecification()
    {
        // Example: Active products AND Price is NOT zero
        // LINQ Equivalent: p => p.IsActive && p.Price != 0
        // Using ! operator for negation
        Where(b => b
            .And(p => p.IsActive)
            .And(p => !(p.Price == 0m)));
    }
}

/// <summary>
/// Specification using Fluent API with grouped conditions: (A AND B) OR (C AND D).
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentGroupedSpecification : Specification<TestProduct>
{
    public FluentGroupedSpecification()
    {
        // Logic: (IsActive AND Price > 50) OR (Category == "Electronics" AND Price < 200)
        // LINQ Equivalent: (IsActive && Price > 50) || (Category == "Electronics" && Price < 200)
        Where(p => p.IsActive)
            .AndGroup(g => g
                .Where(p => p.IsActive)
                .And(p => p.Price > 50m))
            .OrGroup(g => g
                .Where(p => p.Category == "Electronics")
                .And(p => p.Price < 200m));
    }
}

/// <summary>
/// Specification using Fluent API with complex nested groups.
/// Logic: ((IsActive AND Price > 50) OR (Category == "Electronics" AND NOT IsActive)) AND Price < 500
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentComplexNestedSpecification : Specification<TestProduct>
{
    public FluentComplexNestedSpecification()
    {
        // Logic: ((IsActive AND Price > 50) OR (Category == "Electronics" AND NOT IsActive)) AND Price < 500
        // LINQ Equivalent: ((IsActive && Price > 50) || (Category == "Electronics" && !IsActive)) && Price < 500
        Where(b => b
            .AndGroup(g => g
                .Where(p => p.IsActive)
                .And(p => p.Price > 50m)
                .OrGroup(inner => inner
                    .Where(p => p.Category == "Electronics")
                    .And(p => !p.IsActive)))
            .And(p => p.Price < 500m));
    }
}

/// <summary>
/// Specification combining multiple Where calls.
/// Each Where is combined with AND.
/// Pure domain specification - criteria only.
/// </summary>
public sealed class FluentAndLegacyCombinedSpecification : Specification<TestProduct>
{
    public FluentAndLegacyCombinedSpecification()
    {
        Where(p => p.Category == "Electronics");
        Where(p => p.IsActive)
            .Or(p => p.Price > 200m);
    }
}

#endregion
