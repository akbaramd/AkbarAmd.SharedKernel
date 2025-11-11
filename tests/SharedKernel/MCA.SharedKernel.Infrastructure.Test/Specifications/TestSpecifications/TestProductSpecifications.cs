using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

/// <summary>
/// Basic specification for active products.
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
/// Specification for products sorted by price.
/// </summary>
public sealed class ProductsSortedByPriceSpecification : BaseSpecification<TestProduct>
{
    public ProductsSortedByPriceSpecification(bool ascending = true)
    {
        if (ascending)
            AddOrderBy(p => p.Price);
        else
            AddOrderByDescending(p => p.Price);
    }
}

/// <summary>
/// Specification combining multiple criteria.
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
/// Paginated specification for products.
/// </summary>
public sealed class PaginatedProductsSpecification : PaginatedSpecification<TestProduct>
{
    public PaginatedProductsSpecification(int pageNumber, int pageSize)
        : base(pageNumber, pageSize)
    {
    }
}

/// <summary>
/// Paginated specification for active products.
/// </summary>
public sealed class PaginatedActiveProductsSpecification : PaginatedSpecification<TestProduct>
{
    public PaginatedActiveProductsSpecification(int pageNumber, int pageSize)
        : base(pageNumber, pageSize)
    {
        Where(p => p.IsActive);
    }
}

/// <summary>
/// Paginated and sortable specification for products.
/// </summary>
public sealed class PaginatedSortableProductsSpecification : PaginatedSortableSpecification<TestProduct>
{
    public PaginatedSortableProductsSpecification(
        int pageNumber,
        int pageSize,
        Expression<Func<TestProduct, object>>? sortBy = null,
        SortDirection direction = SortDirection.Ascending)
        : base(pageNumber, pageSize, sortBy, direction)
    {
    }
}

/// <summary>
/// Paginated and sortable specification for active products by category.
/// </summary>
public sealed class PaginatedSortableActiveProductsByCategorySpecification : PaginatedSortableSpecification<TestProduct>
{
    public PaginatedSortableActiveProductsByCategorySpecification(
        int pageNumber,
        int pageSize,
        string category,
        Expression<Func<TestProduct, object>>? sortBy = null,
        SortDirection direction = SortDirection.Ascending)
        : base(pageNumber, pageSize, sortBy, direction)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category cannot be null or empty.", nameof(category));

        Where(p => p.IsActive);
        Where(p => p.Category == category);
    }
}

/// <summary>
/// Specification demonstrating OR criteria - products in one category OR another.
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

/// <summary>
/// Specification with include for product reviews using expression.
/// </summary>
public sealed class ProductsWithReviewsSpecification : BaseSpecification<TestProduct>
{
    public ProductsWithReviewsSpecification()
    {
        AddInclude(p => p.Reviews);
    }
}

/// <summary>
/// Specification with include for product reviews using string.
/// </summary>
public sealed class ProductsWithReviewsStringIncludeSpecification : BaseSpecification<TestProduct>
{
    public ProductsWithReviewsStringIncludeSpecification()
    {
        AddInclude("Reviews");
    }
}

/// <summary>
/// Specification combining criteria and includes - active products with reviews.
/// </summary>
public sealed class ActiveProductsWithReviewsSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsWithReviewsSpecification()
    {
        Where(p => p.IsActive);
        AddInclude(p => p.Reviews);
    }
}

/// <summary>
/// Specification with criteria, includes, and sorting - active products with reviews sorted by price.
/// </summary>
public sealed class ActiveProductsWithReviewsSortedByPriceSpecification : BaseSpecification<TestProduct>
{
    public ActiveProductsWithReviewsSortedByPriceSpecification(bool ascending = true)
    {
        Where(p => p.IsActive);
        AddInclude(p => p.Reviews);
        
        if (ascending)
            AddOrderBy(p => p.Price);
        else
            AddOrderByDescending(p => p.Price);
    }
}

/// <summary>
/// Paginated specification with includes - products with reviews.
/// </summary>
public sealed class PaginatedProductsWithReviewsSpecification : PaginatedSpecification<TestProduct>
{
    public PaginatedProductsWithReviewsSpecification(int pageNumber, int pageSize)
        : base(pageNumber, pageSize)
    {
        AddInclude(p => p.Reviews);
    }
}

/// <summary>
/// Paginated and sortable specification with includes - products with reviews, sorted and paginated.
/// </summary>
public sealed class PaginatedSortableProductsWithReviewsSpecification : PaginatedSortableSpecification<TestProduct>
{
    public PaginatedSortableProductsWithReviewsSpecification(
        int pageNumber,
        int pageSize,
        Expression<Func<TestProduct, object>>? sortBy = null,
        SortDirection direction = SortDirection.Ascending)
        : base(pageNumber, pageSize, sortBy, direction)
    {
        AddInclude(p => p.Reviews);
    }
}

/// <summary>
/// Specification with multiple includes using both expression and string includes.
/// </summary>
public sealed class ProductsWithMultipleIncludesSpecification : BaseSpecification<TestProduct>
{
    public ProductsWithMultipleIncludesSpecification()
    {
        AddInclude(p => p.Reviews);
        // Note: In this case, we only have one navigation property, but this demonstrates the pattern
    }
}

/// <summary>
/// Complex specification with criteria, includes, sorting, and pagination.
/// </summary>
public sealed class ComplexProductSpecification : PaginatedSortableSpecification<TestProduct>
{
    public ComplexProductSpecification(
        string category,
        decimal minPrice,
        decimal maxPrice,
        int pageNumber,
        int pageSize,
        Expression<Func<TestProduct, object>>? sortBy = null,
        SortDirection direction = SortDirection.Ascending)
        : base(pageNumber, pageSize, sortBy, direction)
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
        AddInclude(p => p.Reviews);
    }
}

#region Fluent API Specifications

/// <summary>
/// Specification using Fluent API with AND conditions.
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
/// Specification combining multiple AddCriteria calls.
/// Each AddCriteria is combined with AND.
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

#region Multi-Level Sorting Test Specifications

/// <summary>
/// Specification demonstrating multi-level sorting with ThenBy/ThenByDescending.
/// </summary>
public sealed class MultiLevelSortSpecification : BaseSpecification<TestProduct>
{
    public MultiLevelSortSpecification()
    {
        OrderByKey(p => p.Category)
            .ThenByDescending(p => p.Price)
            .ThenBy(p => p.Name);
    }
}

/// <summary>
/// Specification demonstrating NullsLast sorting policy.
/// </summary>
public sealed class NullsLastSortSpecification : BaseSpecification<TestProduct>
{
    public NullsLastSortSpecification()
    {
        OrderByKey(p => p.CreatedAt)
            .NullsLast();
    }
}

/// <summary>
/// Specification demonstrating fluent sorting API.
/// </summary>
public sealed class FluentSortingSpecification : BaseSpecification<TestProduct>
{
    public FluentSortingSpecification()
    {
        Where(p => p.IsActive);
        OrderByKey(p => p.Category)
            .ThenBy(p => p.Price)
            .ThenByDescending(p => p.Name);
    }
}

#endregion

