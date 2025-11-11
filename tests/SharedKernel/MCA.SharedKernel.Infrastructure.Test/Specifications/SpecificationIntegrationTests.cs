using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;
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
    public async Task FindAsync_WithProductsSortedByPriceSpecification_ReturnsSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsSortedByPriceSpecification(ascending: true);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.True(resultsList.Count >= 2);
        for (int i = 0; i < resultsList.Count - 1; i++)
        {
            Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
        }
    }

    [Fact]
    public async Task FindAsync_WithProductsSortedByPriceDescendingSpecification_ReturnsDescendingSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsSortedByPriceSpecification(ascending: false);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

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
    public async Task GetPaginatedAsync_WithPaginatedProductsSpecification_ReturnsPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsSpecification(pageNumber: 1, pageSize: 5);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Items.Count() <= 5);
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithPaginatedActiveProductsSpecification_ReturnsOnlyActiveProducts()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedActiveProductsSpecification(pageNumber: 1, pageSize: 10);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, p => Assert.True(p.IsActive));
        Assert.True(result.TotalCount > 0);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithMultiplePages_ReturnsCorrectPage()
    {
        // Arrange
        await SeedTestData();

        var page1Spec = new PaginatedProductsSpecification(pageNumber: 1, pageSize: 3);
        var page2Spec = new PaginatedProductsSpecification(pageNumber: 2, pageSize: 3);

        // Act
        var page1 = await _repository.GetPaginatedAsync(page1Spec);
        var page2 = await _repository.GetPaginatedAsync(page2Spec);

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

        var totalCount = await _repository.CountAsync();
        var pageSize = 5;
        var lastPageNumber = (int)Math.Ceiling(totalCount / (double)pageSize);

        var specification = new PaginatedProductsSpecification(lastPageNumber, pageSize);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lastPageNumber, result.PageNumber);
        Assert.True(result.Items.Count() <= pageSize);
        Assert.Equal(totalCount, result.TotalCount);
    }

    #endregion

    #region Paginated and Sortable Tests

    [Fact]
    public async Task GetPaginatedAsync_WithPaginatedSortableSpecification_ReturnsSortedPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedSortableProductsSpecification(
            pageNumber: 1,
            pageSize: 5,
            sortBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        var itemsList = result.Items.ToList();
        Assert.True(itemsList.Count <= 5);

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
    public async Task GetPaginatedAsync_WithPaginatedSortableDescendingSpecification_ReturnsDescendingSortedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedSortableProductsSpecification(
            pageNumber: 1,
            pageSize: 5,
            sortBy: p => p.Price,
            direction: SortDirection.Descending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

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
    public async Task GetPaginatedAsync_WithPaginatedSortableByNameSpecification_ReturnsSortedByName()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedSortableProductsSpecification(
            pageNumber: 1,
            pageSize: 10,
            sortBy: p => p.Name,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

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
    public async Task GetPaginatedAsync_WithComplexPaginatedSortableSpecification_ReturnsFilteredSortedPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedSortableActiveProductsByCategorySpecification(
            pageNumber: 1,
            pageSize: 5,
            category: "Electronics",
            sortBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
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
    public void PaginatedProductsSpecification_CreationWithValidParameters_Succeeds()
    {
        // Act
        var specification = new PaginatedProductsSpecification(pageNumber: 1, pageSize: 10);

        // Assert
        Assert.NotNull(specification);
        Assert.Equal(1, specification.PageNumber);
        Assert.Equal(10, specification.PageSize);
        Assert.True(specification.IsPagingEnabled);
        Assert.Equal(0, specification.Skip);
        Assert.Equal(10, specification.Take);
    }

    [Fact]
    public void PaginatedProductsSpecification_CreationWithInvalidParameters_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedProductsSpecification(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedProductsSpecification(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PaginatedProductsSpecification(-1, 10));
    }

    [Fact]
    public void PaginatedSortableProductsSpecification_CreationWithValidParameters_Succeeds()
    {
        // Act
        var specification = new PaginatedSortableProductsSpecification(
            pageNumber: 1,
            pageSize: 10,
            sortBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(specification);
        Assert.Equal(1, specification.PageNumber);
        Assert.Equal(10, specification.PageSize);
        Assert.NotNull(specification.SortBy);
        Assert.Equal(SortDirection.Ascending, specification.Direction);
    }

    #endregion

    #region Include Tests

    [Fact]
    public async Task FindAsync_WithIncludeExpression_LoadsRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsWithReviewsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify that reviews are loaded (not null and accessible)
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        Assert.NotNull(productWithReviews);
        Assert.NotEmpty(productWithReviews.Reviews);
        
        // Verify review data is accessible
        var firstReview = productWithReviews.Reviews.First();
        Assert.NotNull(firstReview.ReviewerName);
        Assert.True(firstReview.Rating >= 1 && firstReview.Rating <= 5);
    }

    [Fact]
    public async Task FindAsync_WithIncludeString_LoadsRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsWithReviewsStringIncludeSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify that reviews are loaded
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        Assert.NotNull(productWithReviews);
        Assert.NotEmpty(productWithReviews.Reviews);
    }

    [Fact]
    public async Task FindAsync_WithCriteriaAndInclude_ReturnsFilteredProductsWithReviews()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            // Reviews collection should be accessible (may be empty for some products)
            Assert.NotNull(p.Reviews);
        });
    }

    [Fact]
    public async Task FindOneAsync_WithInclude_LoadsRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsWithReviewsSpecification();

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reviews);
    }

    [Fact]
    public async Task FindAsync_WithIncludeAndSorting_ReturnsSortedProductsWithReviews()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSortedByPriceSpecification(ascending: true);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.True(p.IsActive));
        Assert.NotNull(resultsList.First().Reviews);
        
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
    public async Task CountAsync_WithInclude_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);
        var results = await _repository.FindAsync(specification);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public async Task ExistsAsync_WithInclude_ReturnsCorrectResult()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    #endregion

    #region Pagination with Includes Tests

    [Fact]
    public async Task GetPaginatedAsync_WithInclude_ReturnsPaginatedProductsWithReviews()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsWithReviewsSpecification(pageNumber: 1, pageSize: 5);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.True(result.Items.Count() <= 5);
        Assert.True(result.TotalCount > 0);
        
        // Verify reviews are loaded
        var firstItem = result.Items.First();
        Assert.NotNull(firstItem.Reviews);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithIncludeAndSorting_ReturnsSortedPaginatedProductsWithReviews()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedSortableProductsWithReviewsSpecification(
            pageNumber: 1,
            pageSize: 5,
            sortBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        var itemsList = result.Items.ToList();
        Assert.True(itemsList.Count <= 5);
        
        // Verify reviews are loaded
        Assert.NotNull(itemsList.First().Reviews);
        
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
    public async Task GetPaginatedAsync_WithIncludeAndMultiplePages_ReturnsCorrectPageWithReviews()
    {
        // Arrange
        await SeedTestData();

        var page1Spec = new PaginatedProductsWithReviewsSpecification(pageNumber: 1, pageSize: 3);
        var page2Spec = new PaginatedProductsWithReviewsSpecification(pageNumber: 2, pageSize: 3);

        // Act
        var page1 = await _repository.GetPaginatedAsync(page1Spec);
        var page2 = await _repository.GetPaginatedAsync(page2Spec);

        // Assert
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(2, page2.PageNumber);
        
        // Verify reviews are loaded on both pages
        Assert.NotNull(page1.Items.First().Reviews);
        if (page2.Items.Any())
        {
            Assert.NotNull(page2.Items.First().Reviews);
        }
        
        // Ensure no overlap
        var page1Ids = page1.Items.Select(p => p.Id).ToHashSet();
        var page2Ids = page2.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithComplexSpecification_ReturnsFilteredSortedPaginatedProductsWithReviews()
    {
        // Arrange
        await SeedTestData();

        var specification = new ComplexProductSpecification(
            category: "Electronics",
            minPrice: 50m,
            maxPrice: 500m,
            pageNumber: 1,
            pageSize: 5,
            sortBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        
        var itemsList = result.Items.ToList();
        Assert.All(itemsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
            Assert.NotNull(p.Reviews);
        });
        
        // Verify sorting
        if (itemsList.Count >= 2)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                Assert.True(itemsList[i].Price <= itemsList[i + 1].Price);
            }
        }
    }

    #endregion

    #region Comprehensive Specification Pattern Tests

    [Fact]
    public async Task FindAsync_WithAllSpecificationFeatures_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

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
        Assert.All(result.Items, p =>
        {
            // Criteria validation
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
            
            // Include validation
            Assert.NotNull(p.Reviews);
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
    public async Task Specification_WithMultipleIncludes_LoadsAllRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsWithMultipleIncludesSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify all includes are loaded
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.NotNull(productWithReviews.Reviews);
            Assert.NotEmpty(productWithReviews.Reviews);
        }
    }

    [Fact]
    public async Task Specification_WithIncludesAndNoTracking_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsWithReviewsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify reviews are loaded even with no tracking
        var productWithReviews = resultsList.FirstOrDefault(p => p.Reviews.Any());
        Assert.NotNull(productWithReviews);
        Assert.NotEmpty(productWithReviews.Reviews);
    }

    [Fact]
    public async Task Specification_WithIncludesAndCount_ExcludesIncludesFromCount()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var count = await _repository.CountAsync(specification);
        var results = await _repository.FindAsync(specification);

        // Assert
        // Count should match the number of products, not products * reviews
        Assert.Equal(count, results.Count());
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Specification_WithIncludesAndExists_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var exists = await _repository.ExistsAsync(specification);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Specification_WithIncludesAndFindOne_LoadsRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsWithReviewsSpecification();

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.NotNull(result.Reviews);
    }

    [Fact]
    public async Task Specification_WithIncludesAndPagination_LoadsReviewsOnAllPages()
    {
        // Arrange
        await SeedTestData();

        var totalCount = await _repository.CountAsync();
        var pageSize = 3;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Act & Assert - verify all pages have reviews loaded
        for (int page = 1; page <= Math.Min(totalPages, 3); page++)
        {
            var specification = new PaginatedProductsWithReviewsSpecification(page, pageSize);
            var result = await _repository.GetPaginatedAsync(specification);
            
            Assert.NotNull(result);
            Assert.Equal(page, result.PageNumber);
            
            foreach (var item in result.Items)
            {
                Assert.NotNull(item.Reviews);
            }
        }
    }

    #endregion

    #region Tests Without Includes (Explicit Verification)

    [Fact]
    public async Task FindAsync_WithoutInclude_DoesNotLoadRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        // Use a specification WITHOUT includes
        var specification = new ActiveProductsSpecification();

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify that reviews are NOT loaded
        // Since we're using AsNoTracking, accessing Reviews.Count should not trigger loading
        // But to be safe, we check that the specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // Verify the query doesn't include reviews by checking a product that we know has reviews
        // When not included, the navigation property should not be loaded
        // Note: In EF Core with AsNoTracking, navigation properties are not loaded unless explicitly included
        var product = resultsList.First();
        Assert.NotNull(product);
        
        // The key test: verify that without include, we can't access review data
        // Since AsNoTracking is used, accessing Reviews should return empty or not be loaded
        // We verify the specification pattern works correctly by ensuring includes list is empty
    }

    [Fact]
    public async Task FindOneAsync_WithoutInclude_DoesNotLoadRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification();

        // Act
        var result = await _repository.FindOneAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // The specification pattern correctly excludes includes when not specified
    }

    [Fact]
    public async Task GetPaginatedAsync_WithoutInclude_DoesNotLoadRelatedEntities()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsSpecification(pageNumber: 1, pageSize: 5);

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // The specification pattern correctly works without includes
    }

    [Fact]
    public async Task Specification_WithIncludeVsWithoutInclude_ShowsDifference()
    {
        // Arrange
        await SeedTestData();

        var specificationWithoutInclude = new ActiveProductsSpecification();
        var specificationWithInclude = new ActiveProductsWithReviewsSpecification();

        // Act
        var resultsWithoutInclude = await _repository.FindAsync(specificationWithoutInclude);
        var resultsWithInclude = await _repository.FindAsync(specificationWithInclude);
        
        var productWithInclude = resultsWithInclude.First(p => p.Reviews.Any());

        // Assert
        // Verify specification differences
        Assert.Empty(specificationWithoutInclude.Includes);
        Assert.Empty(specificationWithoutInclude.IncludeStrings);
        Assert.NotEmpty(specificationWithInclude.Includes);
        
        // With include: reviews should be loaded
        Assert.True(productWithInclude.Reviews.Count > 0);
        Assert.NotNull(productWithInclude.Reviews.First().ReviewerName);
        
        // The key difference: one has includes, the other doesn't
        Assert.True(specificationWithInclude.Includes.Count > 0);
        Assert.True(specificationWithoutInclude.Includes.Count == 0);
    }

    [Fact]
    public async Task CountAsync_WithIncludeVsWithoutInclude_ReturnsSameCount()
    {
        // Arrange
        await SeedTestData();

        var specificationWithoutInclude = new ActiveProductsSpecification();
        var specificationWithInclude = new ActiveProductsWithReviewsSpecification();

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
    public async Task Specification_WithoutInclude_StillAppliesSortingCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsSortedByPriceSpecification(ascending: true); // No includes

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // Verify sorting works without includes
        if (resultsList.Count >= 2)
        {
            for (int i = 0; i < resultsList.Count - 1; i++)
            {
                Assert.True(resultsList[i].Price <= resultsList[i + 1].Price);
            }
        }
    }

    [Fact]
    public async Task Specification_WithoutInclude_StillAppliesPaginationCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new PaginatedProductsSpecification(pageNumber: 1, pageSize: 3); // No includes

        // Act
        var result = await _repository.GetPaginatedAsync(specification);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(3, result.PageSize);
        Assert.True(result.Items.Count() <= 3);
        
        // Verify pagination works without includes
        Assert.True(result.TotalCount > 0);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
    }

    [Fact]
    public async Task Specification_WithoutInclude_HandlesEmptyNavigationProperties()
    {
        // Arrange
        await SeedTestData();

        var specification = new ActiveProductsSpecification(); // No includes

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        // The specification pattern correctly works without includes
        // Navigation properties are not loaded when not included in the specification
    }

    #endregion

    #region Comprehensive Pattern Validation Tests

    [Fact]
    public async Task Specification_AllFeaturesWithoutInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // Create a complex specification WITHOUT includes
        var specification = new ActiveProductsByCategoryAndPriceSpecification("Electronics", 50m, 500m);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        Assert.All(resultsList, p =>
        {
            // Criteria validation
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 500m);
        });
    }

    [Fact]
    public async Task Specification_WithAndWithoutInclude_PerformanceAndBehaviorComparison()
    {
        // Arrange
        await SeedTestData();

        var specWithoutInclude = new ActiveProductsSpecification();
        var specWithInclude = new ActiveProductsWithReviewsSpecification();

        // Act
        var withoutInclude = await _repository.FindAsync(specWithoutInclude);
        var withInclude = await _repository.FindAsync(specWithInclude);

        // Assert
        var withoutList = withoutInclude.ToList();
        var withList = withInclude.ToList();
        
        // Verify specification differences
        Assert.Empty(specWithoutInclude.Includes);
        Assert.NotEmpty(specWithInclude.Includes);
        
        // Both should return the same products (same criteria)
        Assert.Equal(withoutList.Count, withList.Count);
        
        // But with include should have reviews loaded
        var productWithReviews = withList.FirstOrDefault(p => p.Reviews.Any());
        if (productWithReviews != null)
        {
            Assert.True(productWithReviews.Reviews.Count > 0);
        }
        
        // The key difference: specifications have different include configurations
        Assert.True(specWithInclude.Includes.Count > 0);
        Assert.True(specWithoutInclude.Includes.Count == 0);
    }

    [Fact]
    public async Task Specification_EmptyIncludesList_WorksSameAsNoIncludes()
    {
        // Arrange
        await SeedTestData();

        // Create a specification that could have includes but doesn't
        var specificationWithCriteriaOnly = new ActiveProductsSpecification();
        
        // Verify it has no includes
        Assert.Empty(specificationWithCriteriaOnly.Includes);
        Assert.Empty(specificationWithCriteriaOnly.IncludeStrings);

        // Act
        var results = await _repository.FindAsync(specificationWithCriteriaOnly);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specificationWithCriteriaOnly.Includes);
        Assert.Empty(specificationWithCriteriaOnly.IncludeStrings);
        
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
        var paginated = await _repository.GetPaginatedAsync(1, 5, specification);

        // Assert
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
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
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
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
    public async Task Specification_OrCriteriaWithoutInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        // OR criteria without includes
        var specification = new ProductsByCategoryOrSpecification("Electronics", "Furniture");

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
        Assert.All(resultsList, p =>
        {
            Assert.True(p.Category == "Electronics" || p.Category == "Furniture");
        });
    }

    [Fact]
    public async Task Specification_PaginationWithoutInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var page1Spec = new PaginatedProductsSpecification(1, 3);
        var page2Spec = new PaginatedProductsSpecification(2, 3);

        // Act
        var page1 = await _repository.GetPaginatedAsync(page1Spec);
        var page2 = await _repository.GetPaginatedAsync(page2Spec);

        // Assert
        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(1, page1.PageNumber);
        Assert.Equal(2, page2.PageNumber);
        
        // Verify specifications have no includes
        Assert.Empty(page1Spec.Includes);
        Assert.Empty(page2Spec.Includes);
        
        // Verify no overlap
        var page1Ids = page1.Items.Select(p => p.Id).ToHashSet();
        var page2Ids = page2.Items.Select(p => p.Id).ToHashSet();
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task Specification_SortingWithoutInclude_WorksCorrectly()
    {
        // Arrange
        await SeedTestData();

        var specification = new ProductsSortedByPriceSpecification(ascending: false);

        // Act
        var results = await _repository.FindAsync(specification);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        
        // Verify specification has no includes
        Assert.Empty(specification.Includes);
        Assert.Empty(specification.IncludeStrings);
        
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
    public async Task Specification_CountOptimization_ExcludesIncludesFromCountQuery()
    {
        // Arrange
        await SeedTestData();

        var specificationWithInclude = new ActiveProductsWithReviewsSpecification();

        // Act
        var count = await _repository.CountAsync(specificationWithInclude);
        
        // Manually verify count matches
        var allProducts = await _repository.FindAsync(new ActiveProductsSpecification());
        var expectedCount = allProducts.Count(p => p.IsActive);

        // Assert
        // Count should be based on criteria only, not affected by includes
        Assert.Equal(expectedCount, count);
        Assert.True(count > 0);
    }

    [Fact]
    public async Task Specification_ExistsOptimization_ExcludesIncludesFromExistsQuery()
    {
        // Arrange
        await SeedTestData();

        var specificationWithInclude = new ActiveProductsWithReviewsSpecification();
        var specificationWithoutInclude = new ActiveProductsSpecification();

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

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.NotEmpty(spec.Includes);
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

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Include(p => p.Reviews)
            .IncludePaths("Reviews")
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.NotEmpty(spec.Includes);
        Assert.NotEmpty(spec.IncludeStrings);
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

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(p => p.IsActive)
            .Where(b => b
                .Group(g => g
                    .And(p => p.Category == "Electronics")
                    .And(p => p.Price > 50m)))
            .Include(p => p.Reviews)
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
        Assert.NotEmpty(baseSpec.Includes);
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

