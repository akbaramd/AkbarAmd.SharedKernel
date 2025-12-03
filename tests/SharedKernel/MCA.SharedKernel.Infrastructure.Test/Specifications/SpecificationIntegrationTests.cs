using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Repositories;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;
using AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications;
using Microsoft.EntityFrameworkCore;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications;

/// <summary>
/// Comprehensive integration tests for specifications using SQLite in-memory database.
/// Tests criteria-only specifications and repository methods for sorting and pagination.
/// </summary>
public sealed class SpecificationIntegrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly TestProductRepository _repository;
    private readonly TestProductReviewRepository _reviewRepository;

    public SpecificationIntegrationTests(TestDatabaseFixture fixture)
    {
        // Register Infrastructure handlers with Domain layer
        InfrastructureInitialization.RegisterHandlers();
        
        _fixture = fixture;
        _repository = new TestProductRepository(_fixture.DbContext);
        _reviewRepository = new TestProductReviewRepository(_fixture.DbContext);
    }

    #region Basic Specification Tests

    [Fact]
    public async Task FindOneAsync_WithActiveProductsSpecification_ReturnsActiveProduct()
    {
        // Arrange - Clear existing data first to ensure test isolation
        var allProducts = await _repository.FindAsync(p => true);
        await _repository.DeleteRangeAsync(allProducts, save: true);

        var activeProduct = new TestProduct("Active Product", 100m, "Electronics", isActive: true);
        var inactiveProduct = new TestProduct("Inactive Product", 200m, "Electronics", isActive: false);

        await _repository.AddAsync(activeProduct, save: true);
        await _repository.AddAsync(inactiveProduct, save: true);

        var specification = new ActiveProductsSpecification();

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal("Active Product", result.Name);
    }

    [Fact]
    public async Task FindAsync_WithActiveProductsSpecification_ReturnsOnlyActiveProducts()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task FindAsync_WithProductsByCategorySpecification_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategorySpecification("Electronics");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.Equal("Electronics", p.Category));
    }

    [Fact]
    public async Task FindAsync_WithProductsByPriceRangeSpecification_ReturnsOnlyProductsInRange()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByPriceRangeSpecification(100m, 200m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.Price >= 100m);
            Assert.True(p.Price <= 200m);
        });
    }

    [Fact]
    public async Task FindAsync_WithCombinedCriteriaSpecification_ReturnsMatchingProducts()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 100m, 300m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 100m && p.Price <= 300m);
        });
    }

    #endregion

    #region AND Criteria Tests

    [Fact]
    public async Task FindAsync_WithMultipleAndCriteria_ReturnsProductsMatchingAllConditions()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 50m, 200m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m);
            Assert.True(p.Price <= 200m);
        });
    }

    [Fact]
    public async Task FindAsync_WithMultipleAndCriteria_NoResultsWhenConditionsNotMet()
    {
        // Arrange
        await SeedTestData();

        // Search for active Electronics products with price between 1000 and 2000 (none exist)
        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 1000m, 2000m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.Empty(resultsList);
    }

    [Fact]
    public async Task CountAsync_WithMultipleAndCriteria_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 50m, 200m);

        // Act
        var count = await _repository.CountAsync(specification);
        var results = await _repository.FindAsync(specification);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public async Task ExistsAsync_WithMultipleAndCriteria_ReturnsTrueWhenMatchesExist()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 50m, 200m);

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithMultipleAndCriteria_ReturnsFalseWhenNoMatches()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 1000m, 2000m);

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region OR Criteria Tests

    [Fact]
    public async Task FindAsync_WithOrCriteria_ReturnsProductsMatchingEitherCondition()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.Category == "Electronics" || p.Category == "Furniture");
        });

        // Verify we have products from both categories
        var electronicsCount = resultsList.Count(p => p.Category == "Electronics");
        var furnitureCount = resultsList.Count(p => p.Category == "Furniture");
        Assert.True(electronicsCount > 0);
        Assert.True(furnitureCount > 0);
    }

    [Fact]
    public async Task FindAsync_WithPriceRangeOrCategorySpecification_ReturnsMatchingProducts()
    {
        // Arrange
        await SeedTestData();

        // Products with price between 100-200 OR category is Electronics
        var specification = new ProductsByPriceRangeOrCategorySpecification(100m, 200m, "Electronics");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var priceInRange = p.Price >= 100m && p.Price <= 200m;
            var isElectronics = p.Category == "Electronics";
            Assert.True(priceInRange || isElectronics);
        });
    }

    [Fact]
    public async Task FindAsync_WithActiveAndCategoryOrSpecification_ReturnsActiveProductsInEitherCategory()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.True(p.Category == "Electronics" || p.Category == "Furniture");
        });
    }

    [Fact]
    public async Task FindAsync_WithComplexOrSpecification_ReturnsMatchingProducts()
    {
        // Arrange
        await SeedTestData();

        // Active products AND (price < 100 OR category is Electronics)
        var specification = new ActiveProductsByComplexOrSpecification(100m, "Electronics");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            var priceCondition = p.Price < 100m;
            var categoryCondition = p.Category == "Electronics";
            Assert.True(priceCondition || categoryCondition);
        });
    }

    [Fact]
    public async Task CountAsync_WithOrCriteria_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var count = await _repository.CountAsync(specification);
        var results = await _repository.FindAsync(specification);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public async Task ExistsAsync_WithOrCriteria_ReturnsTrueWhenMatchesExist()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithOrCriteria_ReturnsFalseWhenNoMatches()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("NonExistent1", "NonExistent2");

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task FindOneAsync_WithOrCriteria_ReturnsFirstMatchingProduct()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Category == "Electronics" || result.Category == "Furniture");
    }

    [Fact]
    public async Task FindAsync_WithOrCriteriaAndNoMatches_ReturnsEmptyCollection()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategoryOrSpecification("NonExistent1", "NonExistent2");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.Empty(resultsList);
    }

    #endregion

    #region Count and Exists Tests

    [Fact]
    public async Task CountAsync_WithActiveProductsSpecification_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);

        // Assert
        Assert.True(count > 0);
        var allProducts = await _repository.FindAsync(specification);
        Assert.Equal(count, allProducts.Count());
    }

    [Fact]
    public async Task CountAsync_WithProductsByCategorySpecification_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategorySpecification("Electronics");

        // Act
        var count = await _repository.CountAsync(specification);

        // Assert
        Assert.True(count > 0);
        var allProducts = await _repository.FindAsync(specification);
        Assert.Equal(count, allProducts.Count());
    }

    [Fact]
    public async Task ExistsAsync_WithActiveProductsSpecification_ReturnsTrueWhenExists()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithNonMatchingSpecification_ReturnsFalse()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsByCategorySpecification("NonExistentCategory");

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task CountAsync_WithNullSpecification_ReturnsAllCount()
    {
        // Arrange
        await SeedTestData();

        // Act
        var count = await _repository.CountAsync();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    public async Task ExistsAsync_WithNullSpecification_ReturnsTrueIfAnyExist()
    {
        // Arrange
        await SeedTestData();

        // Act
        var exists = await _repository.ExistsAsync(p => true);

        // Assert
        Assert.True(exists);
    }

    #endregion

    #region Pagination Tests

    [Fact]
    public async Task GetPaginatedAsync_WithSpecification_ReturnsPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use new method signature: specification first, then pagination
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Items.Count() <= 5);
        Assert.True(result.TotalCount > 0);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithTruePredicate_ReturnsAllProducts()
    {
        // Arrange
        await SeedTestData();

        // Act - Use predicate that matches all
        var result = await _repository.GetPaginatedAsync(
            predicate: p => true,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.NotNull(p));
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithPredicate_ReturnsPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        // Act - Use predicate instead of specification
        var result = await _repository.GetPaginatedAsync(
            predicate: p => p.IsActive,
            pageNumber: 1,
            pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Items.Count() <= 5);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithMultiplePages_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var page1 = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 3);
        var page2 = await _repository.GetPaginatedAsync(specification, pageNumber: 2, pageSize: 3);

        // Assert
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(2, page2.PageNumber);
        Assert.Equal(3, page1.PageSize);
        Assert.Equal(3, page2.PageSize);

        // Ensure no overlap
        var page1Ids = page1.Items.Select(p => p.Id).ToHashSet();
        var page2Ids = page2.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithLastPage_ReturnsRemainingItems()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();
        var totalCount = await _repository.CountAsync(specification);
        var pageSize = 5;
        var lastPageNumber = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Act
        var result = await _repository.GetPaginatedAsync(specification, lastPageNumber, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lastPageNumber, result.PageNumber);
        Assert.True(result.Items.Count() <= pageSize);
        Assert.Equal(totalCount, result.TotalCount);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithInvalidPageNumber_ThrowsException()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _repository.GetPaginatedAsync(specification, pageNumber: 0, pageSize: 5));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithInvalidPageSize_ThrowsException()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 0));
    }

    #endregion

    #region Sorting Tests

    [Fact]
    public async Task GetPaginatedAsync_WithSpecificationAndSorting_ReturnsSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method with sorting
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);
        var resultsList = result.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(resultsList.Count >= 2);
        for (int i = 0; i < resultsList.Count - 1; i++)
        {
            Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_WithSpecificationAndDescendingSorting_ReturnsDescendingSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method with descending sorting
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Descending);
        var resultsList = result.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(resultsList.Count >= 2);
        for (int i = 0; i < resultsList.Count - 1; i++)
        {
            Assert.True(resultsList[i].Price >= resultsList[i + 1].Price);
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_WithSpecificationAndNameSorting_ReturnsSortedByName()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Name,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(result);
        var itemsList = result.Items.ToList();

        // Verify name sorting
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(string.CompareOrdinal(itemsList[i].Name, itemsList[i + 1].Name) <= 0);
            }
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_WithComplexSpecificationAndSorting_ReturnsFilteredSortedPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 50m, 500m);

        // Act
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 5,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
        });

        var itemsList = result.Items.ToList();
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(itemsList[i].Price <= itemsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_WithPredicateAndSorting_ReturnsSortedResults()
    {
        // Arrange
        await SeedTestData();

        // Act - Use predicate with sorting
        var result = await _repository.GetPaginatedAsync(
            predicate: p => p.IsActive,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);
        var resultsList = result.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.True(p.IsActive));
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
            }
        }
    }

    #endregion

    #region Specification Creation Tests

    [Fact]
    public void ActiveProductsSpecification_Creation_Succeeds()
    {
        // Act
        var specification = new ActiveProductsSpecification();

        // Assert
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
    }

    [Fact]
    public void ProductsByCategorySpecification_CreationWithValidCategory_Succeeds()
    {
        // Act
        var specification = new ProductsByCategorySpecification("Electronics");

        // Assert
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
    }

    [Fact]
    public void ProductsByCategorySpecification_CreationWithEmptyCategory_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ProductsByCategorySpecification(string.Empty));
        Assert.Throws<ArgumentException>(() => new ProductsByCategorySpecification("   "));
    }

    [Fact]
    public void ProductsByPriceRangeSpecification_CreationWithValidRange_Succeeds()
    {
        // Act
        var specification = new ProductsByPriceRangeSpecification(100m, 200m);

        // Assert
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
    }

    [Fact]
    public void ProductsByPriceRangeSpecification_CreationWithInvalidRange_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProductsByPriceRangeSpecification(-1m, 100m));
        Assert.Throws<ArgumentException>(() => new ProductsByPriceRangeSpecification(200m, 100m));
    }

    #endregion

    #region FluentSpecificationBuilder Tests

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_SimpleWhere_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_ComplexWhere_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // LINQ Equivalent: (IsActive && Price > 50) || Category == "Electronics"
        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .AndGroup(g => g
                    .Where(p => p.IsActive)
                    .And(p => p.Price > 50m))
                .Or(p => p.Category == "Electronics"))
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var matchesGroup = p.IsActive && p.Price > 50m;
            var matchesOr = p.Category == "Electronics";
            Assert.True(matchesGroup || matchesOr);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_MultipleWhere_CombinesWithAnd()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(p => p.Category == "Electronics")
            .Where(p => p.Price > 50m)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price > 50m);
        });
    }

    [Fact]
    public void FluentSpecificationBuilder_WhereWithNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .Where((Expression<Func<TestProduct, bool>>)null!)
                .Build();
        });
    }

    [Fact]
    public void FluentSpecificationBuilder_BuildWithNoCriteria_ReturnsNullCriteria()
    {
        // Act
        var spec = new FluentSpecificationBuilder<TestProduct>().Build();

        // Assert
        Assert.NotNull(spec);
        Assert.Null(spec.Criteria);
    }

    #endregion

    #region DDD Specification Pattern Tests (IsSatisfiedBy)

    [Fact]
    public void IsSatisfiedBy_WithActiveProductsSpecification_ReturnsTrueForActiveProduct()
    {
        // Arrange
        var activeProduct = new TestProduct("Active Product", 100m, "Electronics", isActive: true);
        var inactiveProduct = new TestProduct("Inactive Product", 200m, "Electronics", isActive: false);
        var specification = new ActiveProductsSpecification();

      
        // Act & Assert
        Assert.True(specification.IsSatisfiedBy(activeProduct));
        Assert.False(specification.IsSatisfiedBy(inactiveProduct));
    }

    [Fact]
    public void IsSatisfiedBy_WithProductsByCategorySpecification_ReturnsTrueForMatchingCategory()
    {
        // Arrange
        var electronicsProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var furnitureProduct = new TestProduct("Desk", 299m, "Furniture", isActive: true);
        var specification = new ProductsByCategorySpecification("Electronics");

        // Act & Assert
        Assert.True(specification.IsSatisfiedBy(electronicsProduct));
        Assert.False(specification.IsSatisfiedBy(furnitureProduct));
    }

    [Fact]
    public void IsSatisfiedBy_WithComplexSpecification_ReturnsTrueForMatchingProduct()
    {
        // Arrange
        var matchingProduct = new TestProduct("Laptop", 150m, "Electronics", isActive: true);
        var nonMatchingProduct1 = new TestProduct("Mouse", 50m, "Electronics", isActive: true); // Price too low
        var nonMatchingProduct2 = new TestProduct("Monitor", 250m, "Electronics", isActive: false); // Not active
        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 100m, 200m);

        // Act & Assert
        Assert.True(specification.IsSatisfiedBy(matchingProduct));
        Assert.False(specification.IsSatisfiedBy(nonMatchingProduct1));
        Assert.False(specification.IsSatisfiedBy(nonMatchingProduct2));
    }

    [Fact]
    public void IsSatisfiedBy_WithOrCriteria_ReturnsTrueForEitherCondition()
    {
        // Arrange
        var electronicsProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var furnitureProduct = new TestProduct("Desk", 299m, "Furniture", isActive: true);
        var otherProduct = new TestProduct("Book", 19m, "Books", isActive: true);
        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act & Assert
        Assert.True(specification.IsSatisfiedBy(electronicsProduct));
        Assert.True(specification.IsSatisfiedBy(furnitureProduct));
        Assert.False(specification.IsSatisfiedBy(otherProduct));
    }

    [Fact]
    public void IsSatisfiedBy_WithNullCandidate_ThrowsArgumentNullException()
    {
        // Arrange
        var specification = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => specification.IsSatisfiedBy(null!));
    }

    [Fact]
    public void ToExpression_ReturnsSameAsCriteria()
    {
        // Arrange
        var specification = new ActiveProductsSpecification();
        var activeProduct = new TestProduct("Test", 100m, "Category", isActive: true);
        var inactiveProduct = new TestProduct("Test", 100m, "Category", isActive: false);

        // Act
        var criteria = specification.Criteria;
        var expression = specification.ToExpression();

        // Assert
        Assert.NotNull(expression);
        Assert.NotNull(criteria);
        // Both should evaluate to the same result (even if not same reference due to property getter)
        var criteriaCompiled = criteria!.Compile();
        var expressionCompiled = expression!.Compile();
        Assert.Equal(criteriaCompiled(activeProduct), expressionCompiled(activeProduct));
        Assert.Equal(criteriaCompiled(inactiveProduct), expressionCompiled(inactiveProduct));
        // Both should return true for active products
        Assert.True(criteriaCompiled(activeProduct));
        Assert.True(expressionCompiled(activeProduct));
        // Both should return false for inactive products
        Assert.False(criteriaCompiled(inactiveProduct));
        Assert.False(expressionCompiled(inactiveProduct));
    }

    [Fact]
    public void ToExpression_WithFluentSpecificationBuilder_ReturnsCorrectExpression()
    {
        // Arrange
        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(p => p.Price > 50m)
            .Build();

        var matchingProduct = new TestProduct("Product", 100m, "Category", isActive: true);
        var nonMatchingProduct = new TestProduct("Product", 30m, "Category", isActive: true);

        // Act & Assert
        Assert.True(spec.IsSatisfiedBy(matchingProduct));
        Assert.False(spec.IsSatisfiedBy(nonMatchingProduct));
        Assert.NotNull(spec.ToExpression());
    }

    #endregion

    #region Comprehensive Specification Pattern Tests

    [Fact]
    public async Task Specification_WithCriteriaAndRepositoryFeatures_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 50m, 500m);

        // Act - Use repository methods for sorting and pagination
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 10,
            orderBy: p => p.Name,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        Assert.All(result.Items, p =>
        {
            // Criteria validation
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
        });
        
        // Sorting validation
        var itemsList = result.Items.ToList();
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(string.CompareOrdinal(itemsList[i].Name, itemsList[i + 1].Name) <= 0);
            }
        }
        
        // Pagination validation
        Assert.True(itemsList.Count <= 10);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Specification_CriteriaOnly_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.True(p.IsActive));
        // Specifications now only contain criteria - no includes, sorting, or pagination
    }

    [Fact]
    public async Task Specification_WithCriteriaAndCount_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);
        var results = await _repository.FindAsync(specification);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public async Task Specification_WithCriteriaAndExists_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Specification_WithCriteriaAndFindOne_ReturnsMatchingProduct()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Specification_WithCriteriaAndPagination_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();
        var totalCount = await _repository.CountAsync(specification);
        var pageSize = 3;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Act & Assert - verify pagination works with specifications
        for (int page = 1; page <= Math.Min(totalPages, 3); page++)
        {
            var result = await _repository.GetPaginatedAsync(specification, page, pageSize);
            
            Assert.NotNull(result);
            Assert.Equal(page, result.PageNumber);
            Assert.All(result.Items, p => Assert.True(p.IsActive));
        }
    }

    [Fact]
    public async Task FindAsync_WithTruePredicate_ReturnsAllProducts()
    {
        // Arrange
        await SeedTestData();

        // Act - Use predicate that matches all
        var results = await _repository.FindAsync(p => true);
        var resultsList = results.ToList();

        // Assert
        // Should return all products
        Assert.NotEmpty(resultsList);
        var totalCount = await _repository.CountAsync();
        Assert.Equal(totalCount, resultsList.Count);
    }

    [Fact]
    public async Task Specification_AllRepositoryOperations_WorkCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Test all repository methods
        var findOne = await _repository.FindOneAsync(specification);
        var findMany = await _repository.FindAsync(specification);
        var count = await _repository.CountAsync(specification);
        var exists = await _repository.ExistsAsync(specification);
        var paginated = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 5);

        // Assert
        Assert.NotNull(findOne);
        Assert.True(findOne.IsActive);
        
        Assert.NotEmpty(findMany);
        Assert.True(count > 0);
        Assert.True(exists);
        Assert.NotNull(paginated);
        Assert.True(paginated.Items.Any());
        
        // All operations work correctly
        Assert.All(findMany, p => Assert.True(p.IsActive));
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestData()
    {
        // Clear existing data
        var allReviews = await _reviewRepository.FindAsync(r => true);
        await _reviewRepository.DeleteRangeAsync(allReviews, save: false);
        
        var allProducts = await _repository.FindAsync(p => true);
        await _repository.DeleteRangeAsync(allProducts, save: true);

        // Seed test data
        var products = new List<TestProduct>
        {
            new("Laptop", 999.99m, "Electronics", isActive: true),
            new("Mouse", 29.99m, "Electronics", isActive: true),
            new("Keyboard", 79.99m, "Electronics", isActive: true),
            new("Monitor", 299.99m, "Electronics", isActive: false),
            new("Desk Chair", 199.99m, "Furniture", isActive: true),
            new("Office Desk", 399.99m, "Furniture", isActive: true),
            new("Bookshelf", 149.99m, "Furniture", isActive: true),
            new("Table Lamp", 49.99m, "Furniture", isActive: false),
            new("Headphones", 149.99m, "Electronics", isActive: true),
            new("Webcam", 89.99m, "Electronics", isActive: true)
        };

        await _repository.AddRangeAsync(products, save: true);

        // Add reviews for some products
        var reviews = new List<TestProductReview>
        {
            new(products[0], "John Doe", "Great laptop, very fast!", 5, isApproved: true),
            new(products[0], "Jane Smith", "Good value for money", 4, isApproved: true),
            new(products[0], "Bob Johnson", "Battery life could be better", 3, isApproved: true),
            new(products[1], "Alice Brown", "Perfect mouse for gaming", 5, isApproved: true),
            new(products[1], "Charlie Wilson", "Comfortable to use", 4, isApproved: true),
            new(products[4], "David Lee", "Very comfortable chair", 5, isApproved: true),
            new(products[4], "Emma Davis", "Good quality", 4, isApproved: true),
            new(products[5], "Frank Miller", "Sturdy desk", 5, isApproved: true),
            new(products[8], "Grace Taylor", "Excellent sound quality", 5, isApproved: true),
            new(products[8], "Henry White", "Good headphones", 4, isApproved: true),
            new(products[8], "Ivy Black", "Not worth the price", 2, isApproved: false)
        };

        await _reviewRepository.AddRangeAsync(reviews, save: true);
    }

    #endregion
}

/// <summary>
/// Test database fixture for SQLite in-memory database.
/// Provides a shared DbContext instance for all tests in the class.
/// Keeps the connection open to maintain the in-memory database across operations.
/// </summary>
public sealed class TestDatabaseFixture : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    public TestDbContext.TestDbContext DbContext { get; }

    public TestDatabaseFixture()
    {
        // Create and open a connection to keep the in-memory database alive
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext.TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new TestDbContext.TestDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
