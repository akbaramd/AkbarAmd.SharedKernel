using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestSpecifications;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications;

/// <summary>
/// Comprehensive tests for combining multiple specifications using AND, OR, and NOT operations.
/// Tests the Composite Specification pattern implementation.
/// </summary>
public sealed class SpecificationCombinationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly TestProductRepository _repository;

    public SpecificationCombinationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new TestProductRepository(_fixture.DbContext);
    }

    #region AND Combination Tests

    [Fact]
    public async Task FindAsync_WithAndCombinedSpecifications_ReturnsProductsMatchingBoth()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
        });
    }

    [Fact]
    public async Task FindAsync_WithChainedAndCombinations_ReturnsProductsMatchingAll()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var priceSpec = new ProductsByPriceRangeSpecification(50m, 200m);

        var combined = activeSpec
            .And(categorySpec)
            .And(priceSpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 200m);
        });
    }

    [Fact]
    public async Task CountAsync_WithAndCombinedSpecifications_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var count = await _repository.CountAsync(combined);
        var results = await _repository.FindAsync(combined);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public void IsSatisfiedBy_WithAndCombinedSpecifications_ReturnsTrueWhenBothSatisfied()
    {
        // Arrange
        var activeProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var inactiveProduct = new TestProduct("Mouse", 29m, "Electronics", isActive: false);
        var wrongCategoryProduct = new TestProduct("Desk", 199m, "Furniture", isActive: true);

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(activeProduct));
        Assert.False(combined.IsSatisfiedBy(inactiveProduct));
        Assert.False(combined.IsSatisfiedBy(wrongCategoryProduct));
    }

    [Fact]
    public async Task GetPaginatedAsync_WithAndCombinedSpecifications_ReturnsFilteredPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var result = await _repository.GetPaginatedAsync(combined, pageNumber: 1, pageSize: 5);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
        });
    }

    #endregion

    #region OR Combination Tests

    [Fact]
    public async Task FindAsync_WithOrCombinedSpecifications_ReturnsProductsMatchingEither()
    {
        // Arrange
        await SeedTestData();

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var combined = electronicsSpec.Or(furnitureSpec);

        // Act
        var results = await _repository.FindAsync(combined);
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
    public async Task FindAsync_WithChainedOrCombinations_ReturnsProductsMatchingAny()
    {
        // Arrange
        await SeedTestData();

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var priceSpec = new ProductsByPriceRangeSpecification(1000m, 2000m); // No products in this range

        var combined = electronicsSpec
            .Or(furnitureSpec)
            .Or(priceSpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var isElectronics = p.Category == "Electronics";
            var isFurniture = p.Category == "Furniture";
            var isInPriceRange = p.Price >= 1000m && p.Price <= 2000m;
            Assert.True(isElectronics || isFurniture || isInPriceRange);
        });
    }

    [Fact]
    public void IsSatisfiedBy_WithOrCombinedSpecifications_ReturnsTrueWhenEitherSatisfied()
    {
        // Arrange
        var electronicsProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var furnitureProduct = new TestProduct("Desk", 299m, "Furniture", isActive: true);
        var otherProduct = new TestProduct("Book", 19m, "Books", isActive: true);

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var combined = electronicsSpec.Or(furnitureSpec);

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(electronicsProduct));
        Assert.True(combined.IsSatisfiedBy(furnitureProduct));
        Assert.False(combined.IsSatisfiedBy(otherProduct));
    }

    [Fact]
    public async Task CountAsync_WithOrCombinedSpecifications_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var combined = electronicsSpec.Or(furnitureSpec);

        // Act
        var count = await _repository.CountAsync(combined);
        var results = await _repository.FindAsync(combined);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    #endregion

    #region NOT Combination Tests

    [Fact]
    public async Task FindAsync_WithNotSpecification_ReturnsProductsNotMatching()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var notActiveSpec = activeSpec.Not();

        // Act
        var results = await _repository.FindAsync(notActiveSpec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p => Assert.False(p.IsActive));
    }

    [Fact]
    public void IsSatisfiedBy_WithNotSpecification_ReturnsOppositeResult()
    {
        // Arrange
        var activeProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var inactiveProduct = new TestProduct("Mouse", 29m, "Electronics", isActive: false);

        var activeSpec = new ActiveProductsSpecification();
        var notActiveSpec = activeSpec.Not();

        // Act & Assert
        Assert.False(notActiveSpec.IsSatisfiedBy(activeProduct));
        Assert.True(notActiveSpec.IsSatisfiedBy(inactiveProduct));
    }

    [Fact]
    public async Task FindAsync_WithAndNotCombination_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestData();

        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var activeSpec = new ActiveProductsSpecification();
        var notActiveSpec = activeSpec.Not();

        // Electronics AND NOT Active
        var combined = categorySpec.And(notActiveSpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.All(resultsList, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.False(p.IsActive);
        });
    }

    #endregion

    #region Complex Combination Tests

    [Fact]
    public async Task FindAsync_WithComplexAndOrNotCombination_ReturnsCorrectResults()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var priceSpec = new ProductsByPriceRangeSpecification(1000m, 2000m);

        // (Active AND Electronics) OR (Furniture AND NOT PriceRange)
        var combined = activeSpec
            .And(electronicsSpec)
            .Or(furnitureSpec.And(priceSpec.Not()));

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var matchesFirst = p.IsActive && p.Category == "Electronics";
            var matchesSecond = p.Category == "Furniture" && !(p.Price >= 1000m && p.Price <= 2000m);
            Assert.True(matchesFirst || matchesSecond);
        });
    }

    [Fact]
    public async Task FindAsync_WithNestedCombinations_ReturnsCorrectResults()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var priceSpec = new ProductsByPriceRangeSpecification(50m, 200m);

        // Active AND (Electronics OR PriceRange)
        var combined = activeSpec.And(
            electronicsSpec.Or(priceSpec)
        );

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            var isElectronics = p.Category == "Electronics";
            var isInPriceRange = p.Price >= 50m && p.Price <= 200m;
            Assert.True(isElectronics || isInPriceRange);
        });
    }

    [Fact]
    public void IsSatisfiedBy_WithComplexCombination_ReturnsCorrectResult()
    {
        // Arrange
        var matchingProduct1 = new TestProduct("Laptop", 150m, "Electronics", isActive: true);
        var matchingProduct2 = new TestProduct("Mouse", 30m, "Electronics", isActive: true);
        var nonMatchingProduct = new TestProduct("Desk", 500m, "Furniture", isActive: true);

        var activeSpec = new ActiveProductsSpecification();
        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var priceSpec = new ProductsByPriceRangeSpecification(50m, 200m);

        // Active AND (Electronics OR PriceRange)
        var combined = activeSpec.And(electronicsSpec.Or(priceSpec));

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(matchingProduct1)); // Active AND Electronics
        Assert.True(combined.IsSatisfiedBy(matchingProduct2)); // Active AND Electronics
        Assert.False(combined.IsSatisfiedBy(nonMatchingProduct)); // Active but not Electronics and not in price range
    }

    #endregion

    #region Expression Combination Tests

    [Fact]
    public async Task FindAsync_WithSpecAndExpression_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var combined = activeSpec.And(p => p.Price > 100m);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.True(p.Price > 100m);
        });
    }

    [Fact]
    public async Task FindAsync_WithSpecOrExpression_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestData();

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var combined = electronicsSpec.Or(p => p.Price > 500m);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var isElectronics = p.Category == "Electronics";
            var isExpensive = p.Price > 500m;
            Assert.True(isElectronics || isExpensive);
        });
    }

    [Fact]
    public void IsSatisfiedBy_WithSpecAndExpression_ReturnsCorrectResult()
    {
        // Arrange
        var matchingProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var nonMatchingProduct = new TestProduct("Mouse", 29m, "Electronics", isActive: true);

        var activeSpec = new ActiveProductsSpecification();
        var combined = activeSpec.And(p => p.Price > 100m);

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(matchingProduct));
        Assert.False(combined.IsSatisfiedBy(nonMatchingProduct));
    }

    #endregion

    #region AllOf and AnyOf Helper Tests

    [Fact]
    public async Task FindAsync_WithAllOf_ReturnsProductsMatchingAllSpecifications()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var priceSpec = new ProductsByPriceRangeSpecification(50m, 200m);

        var combined = SpecificationExtensions.AllOf(activeSpec, electronicsSpec, priceSpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Price >= 50m && p.Price <= 200m);
        });
    }

    [Fact]
    public async Task FindAsync_WithAnyOf_ReturnsProductsMatchingAnySpecification()
    {
        // Arrange
        await SeedTestData();

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");
        var priceSpec = new ProductsByPriceRangeSpecification(1000m, 2000m);

        var combined = SpecificationExtensions.AnyOf(electronicsSpec, furnitureSpec, priceSpec);

        // Act
        var results = await _repository.FindAsync(combined);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            var isElectronics = p.Category == "Electronics";
            var isFurniture = p.Category == "Furniture";
            var isInPriceRange = p.Price >= 1000m && p.Price <= 2000m;
            Assert.True(isElectronics || isFurniture || isInPriceRange);
        });
    }

    [Fact]
    public void IsSatisfiedBy_WithAllOf_ReturnsTrueWhenAllSatisfied()
    {
        // Arrange
        var matchingProduct = new TestProduct("Laptop", 150m, "Electronics", isActive: true);
        var nonMatchingProduct = new TestProduct("Mouse", 30m, "Electronics", isActive: true);

        var activeSpec = new ActiveProductsSpecification();
        var priceSpec = new ProductsByPriceRangeSpecification(100m, 200m);

        var combined = SpecificationExtensions.AllOf(activeSpec, priceSpec);

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(matchingProduct));
        Assert.False(combined.IsSatisfiedBy(nonMatchingProduct));
    }

    [Fact]
    public void IsSatisfiedBy_WithAnyOf_ReturnsTrueWhenAnySatisfied()
    {
        // Arrange
        var electronicsProduct = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var furnitureProduct = new TestProduct("Desk", 299m, "Furniture", isActive: true);
        var otherProduct = new TestProduct("Book", 19m, "Books", isActive: true);

        var electronicsSpec = new ProductsByCategorySpecification("Electronics");
        var furnitureSpec = new ProductsByCategorySpecification("Furniture");

        var combined = SpecificationExtensions.AnyOf(electronicsSpec, furnitureSpec);

        // Act & Assert
        Assert.True(combined.IsSatisfiedBy(electronicsProduct));
        Assert.True(combined.IsSatisfiedBy(furnitureProduct));
        Assert.False(combined.IsSatisfiedBy(otherProduct));
    }

    [Fact]
    public void AllOf_WithSingleSpecification_ReturnsSameSpecification()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act
        var result = SpecificationExtensions.AllOf(spec);

        // Assert
        Assert.Same(spec, result);
    }

    [Fact]
    public void AllOf_WithEmptyArray_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SpecificationExtensions.AllOf<TestProduct>());
    }

    [Fact]
    public void AnyOf_WithEmptyArray_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SpecificationExtensions.AnyOf<TestProduct>());
    }

    #endregion

    #region Repository Integration Tests

    [Fact]
    public async Task GetPaginatedAsync_WithCombinedSpecification_ReturnsFilteredPaginatedResults()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var result = await _repository.GetPaginatedAsync(
            combined,
            pageNumber: 1,
            pageSize: 3,
            orderBy: p => p.Price,
            direction: SortDirection.Ascending);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items.Any());
        Assert.All(result.Items, p =>
        {
            Assert.True(p.IsActive);
            Assert.Equal("Electronics", p.Category);
        });

        // Verify sorting
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
    public async Task CountAsync_WithCombinedSpecification_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var count = await _repository.CountAsync(combined);
        var results = await _repository.FindAsync(combined);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    [Fact]
    public async Task ExistsAsync_WithCombinedSpecification_ReturnsTrueWhenMatchesExist()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var exists = await _repository.ExistsAsync(combined);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithCombinedSpecification_ReturnsFalseWhenNoMatches()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("NonExistent");
        var combined = activeSpec.And(categorySpec);

        // Act
        var exists = await _repository.ExistsAsync(combined);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task FindOneAsync_WithCombinedSpecification_ReturnsMatchingProduct()
    {
        // Arrange
        await SeedTestData();

        var activeSpec = new ActiveProductsSpecification();
        var categorySpec = new ProductsByCategorySpecification("Electronics");
        var combined = activeSpec.And(categorySpec);

        // Act
        var result = await _repository.FindOneAsync(combined);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal("Electronics", result.Category);
    }

    #endregion

    #region Immutability Tests

    [Fact]
    public void And_ReturnsNewSpecification_OriginalUnchanged()
    {
        // Arrange
        var spec1 = new ActiveProductsSpecification();
        var spec2 = new ProductsByCategorySpecification("Electronics");

        // Act
        var combined = spec1.And(spec2);

        // Assert
        Assert.NotSame(spec1, combined);
        Assert.NotSame(spec2, combined);
        // Original specs should still work independently
        Assert.NotNull(spec1.Criteria);
        Assert.NotNull(spec2.Criteria);
    }

    [Fact]
    public void Or_ReturnsNewSpecification_OriginalUnchanged()
    {
        // Arrange
        var spec1 = new ProductsByCategorySpecification("Electronics");
        var spec2 = new ProductsByCategorySpecification("Furniture");

        // Act
        var combined = spec1.Or(spec2);

        // Assert
        Assert.NotSame(spec1, combined);
        Assert.NotSame(spec2, combined);
        // Original specs should still work independently
        Assert.NotNull(spec1.Criteria);
        Assert.NotNull(spec2.Criteria);
    }

    [Fact]
    public void Not_ReturnsNewSpecification_OriginalUnchanged()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act
        var negated = spec.Not();

        // Assert
        Assert.NotSame(spec, negated);
        // Original spec should still work
        Assert.NotNull(spec.Criteria);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void And_WithNullLeft_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ISpecification<TestProduct>?)null!).And(spec));
    }

    [Fact]
    public void And_WithNullRight_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => spec.And((ISpecification<TestProduct>?)null!));
    }

    [Fact]
    public void Or_WithNullLeft_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ISpecification<TestProduct>?)null!).Or(spec));
    }

    [Fact]
    public void Or_WithNullRight_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => spec.Or((ISpecification<TestProduct>?)null!));
    }

    [Fact]
    public void Not_WithNullSpecification_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ISpecification<TestProduct>?)null!).Not());
    }

    [Fact]
    public void And_WithNullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => spec.And((Expression<Func<TestProduct, bool>>?)null!));
    }

    [Fact]
    public void Or_WithNullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var spec = new ActiveProductsSpecification();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => spec.Or((Expression<Func<TestProduct, bool>>?)null!));
    }

    #endregion

    #region Helper Methods

    private async Task SeedTestData()
    {
        // Clear existing data
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
    }

    #endregion
}

