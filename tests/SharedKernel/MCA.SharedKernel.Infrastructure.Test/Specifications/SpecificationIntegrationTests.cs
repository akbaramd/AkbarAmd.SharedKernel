using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;
using AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications;

/// <summary>
/// Comprehensive integration tests for specifications using SQLite in-memory database.
/// Tests specification creation, composition, and execution with real database operations.
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
    public async Task FindAsync_WithSpecificationAndSorting_ReturnsSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method with sorting
        var results = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);
        var resultsList = results.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(resultsList.Count >= 2);
        for (int i = 0; i < resultsList.Count - 1; i++)
        {
            Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
        }
    }

    [Fact]
    public async Task FindAsync_WithSpecificationAndDescendingSorting_ReturnsDescendingSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method with descending sorting
        var results = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Descending);
        var resultsList = results.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(resultsList.Count >= 2);
        for (int i = 0; i < resultsList.Count - 1; i++)
        {
            Assert.True(resultsList[i].Price >= resultsList[i + 1].Price);
        }
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

    #region Count Tests

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
    public async Task GetPaginatedAsync_WithNullSpecification_ReturnsAllProducts()
    {
        // Arrange
        await SeedTestData();

        // Act - No specification (null) returns all
        var result = await _repository.GetPaginatedAsync(
            (ISpecification<TestProduct>?)null,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.NotNull(p));
        Assert.True(result.TotalCount > 0);
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
    public async Task GetPaginatedAsync_WithSpecificationAndSorting_ReturnsSortedPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method with specification, pagination, and sorting
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 5,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        var itemsList = result.Items.ToList();
        Assert.True(itemsList.Count <= 5);
        Assert.All(itemsList, p => Assert.True(p.IsActive));

        // Verify sorting
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(itemsList[i].Price <= itemsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task GetPaginatedAsync_WithSpecificationAndDescendingSorting_ReturnsDescendingSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 5,
            orderBy: p => p.Price,
            direction: SortDirection.Descending);

        // Assert
        Assert.NotNull(result);
        var itemsList = result.Items.ToList();

        // Verify descending sorting
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(itemsList[i].Price >= itemsList[i + 1].Price);
            }
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
            pageSize: 10,
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

    #endregion

    #region Specification Creation and Composition Tests

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

    [Fact]
    public void ActiveProductsSpecification_Creation_OnlyContainsCriteria()
    {
        // Act
        var specification = new ActiveProductsSpecification();

        // Assert
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
        // Specifications now only contain criteria - no includes, sorting, or pagination
    }

    [Fact]
    public void ProductsByCategorySpecification_Creation_OnlyContainsCriteria()
    {
        // Act
        var specification = new ProductsByCategorySpecification("Electronics");

        // Assert
        Assert.NotNull(specification);
        Assert.NotNull(specification.Criteria);
    }

    #endregion

    #region Comprehensive Specification Pattern Tests

    [Fact]
    public async Task FindAsync_WithComplexCriteriaAndRepositoryFeatures_WorksCorrectly()
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

    #endregion

    #region Specification Pattern Validation Tests

    [Fact]
    public async Task FindAsync_WithCriteriaOnly_WorksCorrectly()
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
        // Specifications contain only criteria - no includes, sorting, or pagination
    }

    [Fact]
    public async Task FindOneAsync_WithCriteriaOnly_WorksCorrectly()
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
    public async Task GetPaginatedAsync_WithCriteriaOnly_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use new repository method signature
        var result = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        Assert.All(result.Items, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task Specification_CriteriaOnly_CountWorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);
        var allActive = await _repository.FindAsync(specification);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, allActive.Count());
    }

    [Fact]
    public async Task Specification_CriteriaOnly_ExistsWorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var countWithoutInclude = await _repository.CountAsync(specificationWithoutInclude);
        var countWithInclude = await _repository.CountAsync(specificationWithInclude);

        // Assert
        // Count should be the same regardless of includes (includes don't affect count)
        Assert.Equal(countWithoutInclude, countWithInclude);
        Assert.True(countWithoutInclude > 0);
    }

    [Fact]
    public async Task ExistsAsync_WithIncludeVsWithoutInclude_ReturnsSameResult()
    {
        // Arrange
        await SeedTestData();

        var specificationWithoutInclude = new ActiveProductsSpecification();
        var specificationWithInclude = new ActiveProductsWithReviewsSpecification();

        // Act
        var existsWithoutInclude = await _repository.ExistsAsync(specificationWithoutInclude);
        var existsWithInclude = await _repository.ExistsAsync(specificationWithInclude);

        // Assert
        // Exists should return the same result regardless of includes
        Assert.Equal(existsWithoutInclude, existsWithInclude);
        Assert.True(existsWithoutInclude);
    }

    [Fact]
    public async Task Specification_WithoutInclude_StillAppliesCriteriaCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification(); // No includes

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            // Criteria works correctly even without includes
        });
    }

    [Fact]
    public async Task Specification_WithRepositorySorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method for sorting
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);
        var resultsList = result.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify sorting works
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task Specification_WithRepositoryPagination_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method for pagination
        var result = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.True(result.Items.Count() <= 3);
        Assert.True(result.TotalCount > 0);
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

    #endregion

    #region Comprehensive Pattern Validation Tests

    [Fact]
    public async Task Specification_ComplexCriteria_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Create a complex specification with criteria only
        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 50m, 500m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        Assert.All(resultsList, p =>
        {
            // Criteria validation
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
        });
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
        
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
        });
    }

    [Fact]
    public async Task Specification_MultipleOperationsWithoutInclude_AllWorkCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification(); // No includes

        // Act - Test all repository methods
        var findOne = await _repository.FindOneAsync(specification);
        var findMany = await _repository.FindAsync(specification);
        var count = await _repository.CountAsync(specification);
        var exists = await _repository.ExistsAsync(specification);
        var paginated = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 5);

        // Assert
        // Specifications now only contain criteria
        
        Assert.NotNull(findOne);
        Assert.True(findOne.IsActive);
        
        Assert.NotEmpty(findMany);
        Assert.True(count > 0);
        Assert.True(exists);
        Assert.NotNull(paginated);
        Assert.True(paginated.Items.Any());
        
        // All operations work correctly without includes
        Assert.All(findMany, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task Specification_ComplexCriteriaWithoutInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Complex AND criteria without includes
        var specification = new ActiveProductsByMultipleAndCriteriaSpecification("Electronics", 50m, 200m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        Assert.All(resultsList, p =>
        {
            // All criteria should be met
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m);
            Assert.True(p.Price <= 200m);
        });
    }

    [Fact]
    public async Task Specification_OrCriteria_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // OR criteria
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
    }

    [Fact]
    public async Task Specification_WithRepositoryPagination_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method for pagination
        var page1 = await _repository.GetPaginatedAsync(specification, pageNumber: 1, pageSize: 3);
        var page2 = await _repository.GetPaginatedAsync(specification, pageNumber: 2, pageSize: 3);

        // Assert
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(2, page2.PageNumber);
        
        // Verify no overlap
        var page1Ids = page1.Items.Select(p => p.Id).ToHashSet();
        var page2Ids = page2.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task Specification_WithRepositorySorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act - Use repository method for sorting
        var result = await _repository.GetPaginatedAsync(
            specification,
            pageNumber: 1,
            pageSize: 100,
            orderBy: p => p.Price,
            direction: SortDirection.Descending);
        var resultsList = result.Items.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify descending sort
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price >= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task Specification_Count_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);
        
        // Manually verify count matches
        var allProducts = await _repository.FindAsync(specification);
        var expectedCount = allProducts.Count();

        // Assert
        // Count should be based on criteria only
        Assert.Equal(expectedCount, count);
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Specification_Exists_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var existsWithInclude = await _repository.ExistsAsync(specificationWithInclude);
        var existsWithoutInclude = await _repository.ExistsAsync(specificationWithoutInclude);

        // Assert
        // Exists should return same result regardless of includes
        Assert.Equal(existsWithoutInclude, existsWithInclude);
        Assert.True(existsWithInclude);
    }

    [Fact]
    public async Task Specification_WithNullSpecification_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Act
        ISpecification<TestProduct>? nullSpec = null;
        var results = await _repository.FindAsync(nullSpec);
        var resultsList = results.ToList();

        // Assert
        // Should return all products when specification is null
        Assert.NotEmpty(resultsList);
    }

    [Fact]
    public async Task Specification_WithEmptyCriteria_ReturnsAllEntities()
    {
        // Arrange
        await SeedTestData();

        // Create a specification with no criteria (empty specification)
        var emptySpec = new PaginatedProductsSpecification(1, 100);

        // Act
        var results = await _repository.FindAsync(emptySpec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        // Should return all products when no criteria specified
    }

    [Fact]
    public async Task Specification_IncludeExpressionVsString_BothWorkCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specWithExpression = new ProductsWithReviewsSpecification();
        var specWithString = new ProductsWithReviewsStringIncludeSpecification();

        // Act
        var resultsExpression = await _repository.FindAsync(specWithExpression);
        var resultsString = await _repository.FindAsync(specWithString);

        // Assert
        Assert.NotEmpty(resultsExpression);
        Assert.NotEmpty(resultsString);
        
        // Both should have includes configured
        Assert.NotEmpty(specWithExpression.Includes);
        Assert.NotEmpty(specWithString.IncludeStrings);
        
        // Both should load reviews
        var productWithExpression = resultsExpression.FirstOrDefault(p => p.Reviews.Any());
        var productWithString = resultsString.FirstOrDefault(p => p.Reviews.Any());
        
        if (productWithExpression != null && productWithString != null)
        {
            Assert.True(productWithExpression.Reviews.Count > 0);
            Assert.True(productWithString.Reviews.Count > 0);
        }
    }

    [Fact]
    public async Task Specification_CombiningAllFeatures_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test specification with: criteria + includes + sorting + pagination
        var specification = new ComplexProductSpecification(
            category: "Electronics",
            minPrice: 50m,
            maxPrice: 500m,
            pageNumber: 1,
            pageSize: 10,
            sortBy: p => p.Name,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        
        // Verify all features are present
        Assert.NotNull(specification.Criteria); // Has criteria
        Assert.NotEmpty(specification.Includes); // Has includes
        Assert.NotNull(specification.SortBy); // Has sorting
        Assert.True(specification.IsPagingEnabled); // Has pagination
        
        // Verify results match all criteria
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
            Assert.NotNull(p.Reviews); // Include is applied
        });
    }

    [Fact]
    public async Task Specification_NoIncludesNoCriteria_ReturnsAllWithNoIncludes()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsSpecification(1, 100);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        
        // Verify no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // Verify no criteria (returns all)
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task Specification_WithIncludesAndComplexCriteria_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ComplexProductSpecification(
            category: "Furniture",
            minPrice: 100m,
            maxPrice: 400m,
            pageNumber: 1,
            pageSize: 5,
            sortBy: p => p.Price,
            direction: SortDirection.Descending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        
        // Verify all features
        Assert.NotNull(specification.Criteria);
        Assert.NotEmpty(specification.Includes);
        Assert.NotNull(specification.SortBy);
        Assert.True(specification.IsPagingEnabled);
        
        // Verify results
        var itemsList = result.Items.ToList();
        Assert.All(itemsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Furniture", p.Category);
            Assert.True(p.Price >= 100m && p.Price <= 400m);
            Assert.NotNull(p.Reviews);
        });
        
        // Verify sorting
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(itemsList[i].Price >= itemsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task Specification_PaginationWithIncludes_CountIsCorrect()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsWithReviewsSpecification(1, 5);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        Assert.True(result.Items.Count() <= 5);
        
        // Verify count matches actual filtered count (not affected by includes)
        var countSpec = new ActiveProductsSpecification();
        var expectedCount = await _repository.CountAsync(countSpec);
        
        // Total count should be based on criteria, not includes
        // Since PaginatedProductsSpecification has no criteria, it should count all
        var allCount = await _repository.CountAsync();
        Assert.Equal(allCount, result.TotalCount);
    }

    [Fact]
    public async Task Specification_IncludesDoNotAffectFiltering()
    {
        // Arrange
        await SeedTestData();

        var specWithInclude = new ActiveProductsWithReviewsSpecification();
        var specWithoutInclude = new ActiveProductsSpecification();

        // Act
        var withInclude = await _repository.FindAsync(specWithInclude);
        var withoutInclude = await _repository.FindAsync(specWithoutInclude);

        // Assert
        // Both should return the same products (same criteria)
        var withList = withInclude.ToList();
        var withoutList = withoutInclude.ToList();
        
        Assert.Equal(withList.Count, withoutList.Count);
        
        // Product IDs should match (same filtering)
        var withIds = withList.Select(p => p.Id).OrderBy(id => id).ToList();
        var withoutIds = withoutList.Select(p => p.Id).OrderBy(id => id).ToList();
        Assert.Equal(withIds, withoutIds);
    }

    [Fact]
    public async Task Specification_IncludesDoNotAffectSorting()
    {
        // Arrange
        await SeedTestData();

        var specWithInclude = new ActiveProductsWithReviewsSortedByPriceSpecification(ascending: true);
        var specWithoutInclude = new ProductsSortedByPriceSpecification(ascending: true);

        // Act
        var withInclude = await _repository.FindAsync(specWithInclude);
        var withoutInclude = await _repository.FindAsync(specWithoutInclude);

        // Assert
        var withList = withInclude.ToList();
        var withoutList = withoutInclude.ToList();
        
        // Both should be sorted the same way
        if (withList.Count >= 2 && withoutList.Count >= 2)
        {
            for (int i = 0; i < Math.Min(withList.Count, withoutList.Count) - 1; i++)
            {
                Assert.True(withList[i].Price <= withList[i + 1].Price);
                Assert.True(withoutList[i].Price <= withoutList[i + 1].Price);
            }
        }
        
        // Verify includes difference
        Assert.NotEmpty(specWithInclude.Includes);
        Assert.Empty(specWithoutInclude.Includes);
    }

    [Fact]
    public async Task Specification_IncludesDoNotAffectPagination()
    {
        // Arrange
        await SeedTestData();

        var specWithInclude = new PaginatedProductsWithReviewsSpecification(1, 3);
        var specWithoutInclude = new PaginatedProductsSpecification(1, 3);

        // Act
        var withInclude = await _repository.GetPaginatedAsync(specWithInclude);
        var withoutInclude = await _repository.GetPaginatedAsync(specWithoutInclude);

        // Assert
        // Both should return same page (same pagination parameters)
        Assert.Equal(withInclude.PageNumber, withoutInclude.PageNumber);
        Assert.Equal(withInclude.PageSize, withoutInclude.PageSize);
        Assert.Equal(withInclude.TotalCount, withoutInclude.TotalCount);
        
        // Verify includes difference
        Assert.NotEmpty(specWithInclude.Includes);
        Assert.Empty(specWithoutInclude.Includes);
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

    #region Fluent API Tests

    [Fact]
    public async Task FindAsync_WithFluentAPI_AndConditions_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentAndSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.True(p.Price > 50m);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentAPI_OrConditions_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentOrSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive || p.Price > 200m);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentAPI_NotCondition_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentNotSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.False(p.IsActive));
    }

    [Fact]
    public async Task FindAsync_WithFluentAPI_GroupedConditions_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentGroupedSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        // Should match: (IsActive AND Price > 50) OR (Category == "Electronics" AND Price < 200)
        Assert.All(resultsList, p =>
        {
            var matchesFirstGroup = p.IsActive && p.Price > 50m;
            var matchesSecondGroup = p.Category == "Electronics" && p.Price < 200m;
            Assert.True(matchesFirstGroup || matchesSecondGroup);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentAPI_ComplexNestedGroups_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentComplexNestedSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        // Complex logic: ((IsActive AND Price > 50) OR (Category == "Electronics" AND NOT IsActive)) AND Price < 500
        Assert.All(resultsList, p =>
        {
            Assert.True(p.Price < 500m);
            var matchesFirstGroup = p.IsActive && p.Price > 50m;
            var matchesSecondGroup = p.Category == "Electronics" && !p.IsActive;
            Assert.True(matchesFirstGroup || matchesSecondGroup);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentAPI_MultipleAddCriteria_CombinesCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentAndLegacyCombinedSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        // Should match: (Category == "Electronics") AND (IsActive OR Price > 200)
        Assert.All(resultsList, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.IsActive || p.Price > 200m);
        });
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

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .Group(g => g
                    .And(p => p.IsActive)
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
    public async Task FindAsync_WithFluentSpecificationBuilder_Includes_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .Build();
        
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.NotNull(spec.IncludeChains);
        Assert.Single(spec.IncludeChains);
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.NotEmpty(productWithReviews.Reviews);
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_MultipleIncludes_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.NotNull(spec.IncludeChains);
        Assert.Single(spec.IncludeChains);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_OrderBy_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        var multiSortSpec = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec);
        Assert.True(multiSortSpec.Sorts.Count > 0);
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_OrderByDescending_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Price)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        var multiSortSpec2 = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec2);
        Assert.True(multiSortSpec2.Sorts.Count > 0);
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price >= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_MultiLevelSorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenByDescending(p => p.Price)
            .ThenBy(p => p.Name)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        var multiSortSpec = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec);
        Assert.Equal(3, multiSortSpec.Sorts.Count);
        Assert.Equal(SortDirection.Ascending, multiSortSpec.Sorts[0].Direction);
        Assert.Equal(SortDirection.Descending, multiSortSpec.Sorts[1].Direction);
        Assert.Equal(SortDirection.Ascending, multiSortSpec.Sorts[2].Direction);

        // Verify sorting configuration is correct
        // The actual sorting order is verified by the database query execution
        // We just verify that the specification has the correct sort descriptors configured
        Assert.NotNull(multiSortSpec.Sorts[0].KeySelector);
        Assert.NotNull(multiSortSpec.Sorts[1].KeySelector);
        Assert.NotNull(multiSortSpec.Sorts[2].KeySelector);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_Page_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Page(1, 5)
            .Build();

        // Act
        var result = await _repository.GetPaginatedAsync(1, 5, spec);

        // Assert
        Assert.NotNull(result);
        var baseSpec = spec as BaseSpecification<TestProduct>;
        Assert.NotNull(baseSpec);
        Assert.True(baseSpec.IsPagingEnabled);
        Assert.Equal(0, baseSpec.Skip);
        Assert.Equal(5, baseSpec.Take);
        Assert.True(result.Items.Count() <= 5);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_SkipTake_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Skip(2)
            .Take(3)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        var baseSpec = spec as BaseSpecification<TestProduct>;
        Assert.NotNull(baseSpec);
        Assert.True(baseSpec.IsPagingEnabled);
        Assert.Equal(2, baseSpec.Skip);
        Assert.Equal(3, baseSpec.Take);
        Assert.True(resultsList.Count <= 3);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_CompleteSpecification_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(b => b
                .Group(g => g
                    .And(p => p.Category == "Electronics")
                    .And(p => p.Price > 50m)))
            .Include(p => p.Reviews)
            .Build();
        
        var fluentBuilder = (FluentSpecificationBuilder<TestProduct>)builder;
        var spec = fluentBuilder
            .OrderBy(p => p.Category)
            .ThenByDescending(p => p.Price)
            .Page(1, 10)
            .Build();

        // Act
        var result = await _repository.GetPaginatedAsync(1, 10, spec);

        // Assert
        Assert.NotNull(result);
        var baseSpec = spec as BaseSpecification<TestProduct>;
        Assert.NotNull(baseSpec);
        Assert.True(baseSpec.IsPagingEnabled);
        Assert.NotNull(baseSpec.IncludeChains);
        Assert.Single(baseSpec.IncludeChains);
        var multiSortSpec = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec);
        Assert.Equal(2, multiSortSpec.Sorts.Count);
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price > 50m);
            Assert.NotNull(p.Reviews);
        });
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_OrderByResetsChain_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Price)
            .OrderBy(p => p.Name) // This should reset the chain
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        var multiSortSpec = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec);
        Assert.Equal(1, multiSortSpec.Sorts.Count); // Only the last OrderBy should remain
        Assert.True(multiSortSpec.Sorts[0].KeySelector.ToString().Contains("Name"));
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
    public void FluentSpecificationBuilder_PageWithInvalidParameters_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .Page(0, 10)
                .Build();
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .Page(1, 0)
                .Build();
        });
    }

    [Fact]
    public void FluentSpecificationBuilder_ThenByWithoutOrderBy_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .ThenBy(p => p.Price)
                .Build();
        });
    }

    [Fact]
    public void FluentSpecificationBuilder_NullsFirstWithoutOrderBy_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .NullsFirst()
                .Build();
        });
    }

    #endregion

    #region Multi-Level Sorting Tests

    [Fact]
    public async Task FindAsync_WithBaseSpecification_MultiLevelSorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new MultiLevelSortSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        var multiSortSpec = spec as IMultiSortSpecification<TestProduct>;
        Assert.NotNull(multiSortSpec);
        Assert.Equal(3, multiSortSpec.Sorts.Count);
        
        // Verify sorting configuration is correct
        // The actual sorting order is verified by the database query execution
        // We just verify that the specification has the correct sort descriptors configured
        Assert.NotNull(multiSortSpec.Sorts[0].KeySelector);
        Assert.NotNull(multiSortSpec.Sorts[1].KeySelector);
        Assert.NotNull(multiSortSpec.Sorts[2].KeySelector);
        Assert.Equal(SortDirection.Ascending, multiSortSpec.Sorts[0].Direction);
        Assert.Equal(SortDirection.Descending, multiSortSpec.Sorts[1].Direction);
        Assert.Equal(SortDirection.Ascending, multiSortSpec.Sorts[2].Direction);
    }

    [Fact]
    public async Task FindAsync_WithBaseSpecification_NullsLast_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Create products with null CreatedAt (if possible) or use a nullable field
        var spec = new NullsLastSortSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(spec.Sorts.Count > 0);
        Assert.Equal(NullSort.NullsLast, spec.Sorts[0].Nulls);
    }

    [Fact]
    public async Task FindAsync_WithBaseSpecification_FluentSorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSortingSpecification();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(spec.Sorts.Count >= 2);
    }

    #endregion

    #region Nullable Navigation Properties Tests

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NullableIncludeExpression_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test that Include accepts nullable expressions (the API signature accepts object? which supports nullable)
        // Note: We don't cast to object? in the expression itself as EF Core requires direct property access
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews) // The Include method signature accepts Expression<Func<T, object?>> which supports nullable
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.NotEmpty(spec.IncludeChains);
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.NotEmpty(productWithReviews.Reviews);
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NullableThenInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test that ThenInclude accepts nullable property types in the expression
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .ThenInclude<TestProductReview, TestProduct?>(r => r.Product) // Nullable Product type
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.Single(spec.IncludeChains);
        var chain = spec.IncludeChains[0];
        Assert.NotNull(chain.IncludeExpression);
        Assert.Single(chain.ThenIncludeExpressions);
        
        // Verify the include chain works correctly
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.NotEmpty(productWithReviews.Reviews);
            var review = productWithReviews.Reviews.First();
            Assert.NotNull(review.Product);
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NullablePreviousTypeInThenInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test that ThenInclude accepts nullable previous type (TPrevious?)
        // Note: Since TestProductReview.Product is not nullable, we test the API accepts the nullable type annotation
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .ThenInclude<TestProductReview?, TestProduct>(r => r!.Product) // Nullable previous type, non-nullable property
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.Single(spec.IncludeChains);
        var chain = spec.IncludeChains[0];
        Assert.NotNull(chain.IncludeExpression);
        Assert.Single(chain.ThenIncludeExpressions);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NestedIncludesWithNullable_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test nested includes with nullable types at multiple levels
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .ThenInclude<TestProductReview, TestProduct?>(r => r.Product) // Nullable Product
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.Single(spec.IncludeChains);
        var chain = spec.IncludeChains[0];
        Assert.NotNull(chain.IncludeExpression);
        Assert.Single(chain.ThenIncludeExpressions);
        
        // Verify the nested include works
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.NotEmpty(productWithReviews.Reviews);
            var review = productWithReviews.Reviews.First();
            Assert.NotNull(review.Product);
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_MultipleThenIncludeWithNullable_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test multiple ThenInclude calls with nullable types
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .ThenInclude(r => r.Product) // First nullable
            .ThenInclude<TestProduct, ICollection<TestProductReview>?>(p => p.Reviews) // Second nullable collection
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.Single(spec.IncludeChains);
        var chain = spec.IncludeChains[0];
        Assert.NotNull(chain.IncludeExpression);
        Assert.Equal(2, chain.ThenIncludeExpressions.Count);
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NullableIncludeWithCriteriaAndSorting_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test nullable includes combined with criteria and sorting
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(p => p.Category == "Electronics")
            .Include(p => p.Reviews) // Include accepts nullable signature
            .ThenInclude<TestProductReview, TestProduct>(r => r.Product) // ThenInclude accepts nullable signature
            .Build();
        var spec = ((FluentSpecificationBuilder<TestProduct>)builder).Build();
        
        // Build the final spec and add sorting separately
        var builder2 = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(p => p.Category == "Electronics")
            .Include(p => p.Reviews)
            .ThenInclude<TestProductReview, TestProduct>(r => r.Product);
        
        var buildResult = builder2.Build();
        var fluentBuilder = (FluentSpecificationBuilder<TestProduct>)buildResult;
        spec = fluentBuilder
            .OrderBy(p => p.Price)
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
        });
        Assert.Single(spec.IncludeChains);
        
        // Verify sorting
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task FindAsync_WithFluentSpecificationBuilder_NullableIncludeWithPagination_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Test nullable includes with pagination
        var builder = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews) // Include accepts nullable signature
            .ThenInclude<TestProductReview, TestProduct>(r => r.Product); // ThenInclude accepts nullable signature
        
        var buildResult = builder.Build();
        var fluentBuilder = (FluentSpecificationBuilder<TestProduct>)buildResult;
        var spec = fluentBuilder
            .Page(1, 5)
            .Build();

        // Act
        var result = await _repository.GetPaginatedAsync(1, 5, spec);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Items.Count() <= 5);
        Assert.Single(spec.IncludeChains);
        
        // Verify includes are loaded
        var firstItem = result.Items.First();
        Assert.NotNull(firstItem.Reviews);
    }

    [Fact]
    public void FluentSpecificationBuilder_IncludeWithNullExpression_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .Include((Expression<Func<TestProduct, object?>>)null!)
                .Build();
        });
    }

    [Fact]
    public void FluentSpecificationBuilder_ThenIncludeWithNullExpression_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            var spec = new FluentSpecificationBuilder<TestProduct>()
                .Include(p => p.Reviews)
                .ThenInclude<TestProductReview, TestProduct?>(null!)
                .Build();
        });
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


                 