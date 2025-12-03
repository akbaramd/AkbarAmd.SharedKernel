using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications;

/// <summary>
/// Tests for the new Group pattern that requires Where() as the first operation.
/// Demonstrates the pattern: AndGroup(g => g.Where(...).Or(...).Or(...))
/// </summary>
public sealed class GroupWherePatternTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly TestProductRepository _repository;

    public GroupWherePatternTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new TestProductRepository(_fixture.DbContext);
    }

    #region Group with Where Pattern Tests

    [Fact]
    public async Task FindAsync_WithGroupStartingWithWhere_ReturnsCorrectResults()
    {
        // Arrange
        await SeedTestData();

        // Pattern: Where(b => b.And(...).And(...).AndGroup(g => g.Where(...).Or(...).Or(...)))
        // LINQ Equivalent:
        // var query = entities.Where(b => 
        //     restrictedTourIds.Contains(b.TourId) &&
        //     b.Participants.Any(p => p.NationalNumber == nationalNumber) &&
        //     (b.Status == TourReservationStatus.Draft ||
        //      (b.Status == TourReservationStatus.OnHold && b.ExpiryDate.HasValue && b.ExpiryDate.Value > DateTime.UtcNow) ||
        //      (b.Status == TourReservationStatus.Waitlisted && b.ExpiryDate.HasValue && b.ExpiryDate.Value > DateTime.UtcNow) ||
        //      b.Status == TourReservationStatus.Confirmed)
        // );
        var restrictedCategoryIds = new[] { "Electronics", "Furniture" };
        var minPrice = 50m;

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .And(r => restrictedCategoryIds.Contains(r.Category))
                .And(r => r.Price >= minPrice)
                .AndGroup(g => g
                    .Where(r => r.IsActive)
                    .Or(r => r.Price > 100m)
                    .Or(r => r.Category == "Electronics"))
            )
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.Contains(p.Category, restrictedCategoryIds);
            Assert.True(p.Price >= minPrice);
            // Must satisfy: (IsActive OR Price > 100 OR Category == "Electronics")
            var satisfiesGroup = p.IsActive || p.Price > 100m || p.Category == "Electronics";
            Assert.True(satisfiesGroup);
        });
    }

    [Fact]
    public async Task FindAsync_WithComplexGroupPattern_ReturnsCorrectResults()
    {
        // Arrange
        await SeedTestData();

        // Pattern matching the user's example structure:
        // Where(b => b
        //     .And(r => restrictedTourIds.Contains(r.TourId))
        //     .And(r => r.Participants.Any(p => p.NationalNumber == nationalNumber))
        //     .AndGroup(g => g
        //         Where(r => r.Status == TourReservationStatus.Draft)
        //         .Or(r => r.Status == TourReservationStatus.OnHold && r.ExpiryDate.HasValue && r.ExpiryDate.Value > DateTime.UtcNow)
        //         .Or(r => r.Status == TourReservationStatus.Waitlisted && r.ExpiryDate.HasValue && r.ExpiryDate.Value > DateTime.UtcNow)
        //         .Or(r => r.Status == TourReservationStatus.Confirmed)
        //     )
        // )
        // 
        // LINQ Equivalent:
        // var query = entities.Where(b => 
        //     restrictedTourIds.Contains(b.TourId) &&
        //     b.Participants.Any(p => p.NationalNumber == nationalNumber) &&
        //     (b.Status == TourReservationStatus.Draft ||
        //      (b.Status == TourReservationStatus.OnHold && b.ExpiryDate.HasValue && b.ExpiryDate.Value > DateTime.UtcNow) ||
        //      (b.Status == TourReservationStatus.Waitlisted && b.ExpiryDate.HasValue && b.ExpiryDate.Value > DateTime.UtcNow) ||
        //      b.Status == TourReservationStatus.Confirmed)
        // );
        var restrictedCategories = new[] { "Electronics", "Furniture" };
        var now = DateTime.UtcNow;

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .And(r => restrictedCategories.Contains(r.Category))
                .And(r => r.Price >= 50m)
                .AndGroup(g => g
                    .Where(r => r.IsActive)
                    .Or(r => r.Category == "Electronics" && r.Price > 100m)
                    .Or(r => r.Category == "Furniture" && r.CreatedAt > now.AddDays(-30))
                    .Or(r => r.Price > 200m))
            )
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.Contains(p.Category, restrictedCategories);
            Assert.True(p.Price >= 50m);
            // Must satisfy the group condition:
            var satisfiesGroup = p.IsActive ||
                                 (p.Category == "Electronics" && p.Price > 100m) ||
                                 (p.Category == "Furniture" && p.CreatedAt > now.AddDays(-30)) ||
                                 p.Price > 200m;
            Assert.True(satisfiesGroup);
        });
    }

    [Fact]
    public void IsSatisfiedBy_WithGroupStartingWithWhere_ReturnsCorrectResult()
    {
        // Arrange
        var product1 = new TestProduct("Laptop", 999m, "Electronics", isActive: true);
        var product2 = new TestProduct("Mouse", 29m, "Electronics", isActive: false);
        var product3 = new TestProduct("Desk", 199m, "Furniture", isActive: true);

        var restrictedCategories = new[] { "Electronics", "Furniture" };

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .And(r => restrictedCategories.Contains(r.Category))
                .AndGroup(g => g
                    .Where(r => r.IsActive)
                    .Or(r => r.Price > 100m))
            )
            .Build();

        // Act & Assert
        // Product1: Electronics, IsActive=true -> satisfies group
        Assert.True(spec.IsSatisfiedBy(product1));
        
        // Product2: Electronics, IsActive=false, Price=29 -> doesn't satisfy group
        Assert.False(spec.IsSatisfiedBy(product2));
        
        // Product3: Furniture, IsActive=true -> satisfies group
        Assert.True(spec.IsSatisfiedBy(product3));
    }

    [Fact]
    public async Task CountAsync_WithGroupStartingWithWhere_ReturnsCorrectCount()
    {
        // Arrange
        await SeedTestData();

        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .And(r => r.Category == "Electronics")
                .AndGroup(g => g
                    .Where(r => r.IsActive)
                    .Or(r => r.Price > 100m))
            )
            .Build();

        // Act
        var count = await _repository.CountAsync(spec);
        var results = await _repository.FindAsync(spec);

        // Assert
        Assert.True(count > 0);
        Assert.Equal(count, results.Count());
    }

    #endregion

    #region Validation Tests - Groups Must Start with Where

    [Fact]
    public void Group_StartingWithAnd_ThrowsInvalidOperationException()
    {
        // Arrange
        var spec = new FluentSpecificationBuilder<TestProduct>();

        // Act & Assert
        // This should throw because groups must start with Where(), not And()
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            spec.Where(b => b
                .AndGroup(g => g
                    .And(r => r.IsActive) // ❌ Should start with Where, not And
                    .Or(r => r.Price > 100m))
            );
        });

        Assert.Contains("Groups must start with Where()", exception.Message);
    }

    [Fact]
    public void Group_StartingWithOr_ThrowsInvalidOperationException()
    {
        // Arrange
        var spec = new FluentSpecificationBuilder<TestProduct>();

        // Act & Assert
        // This should throw because groups must start with Where(), not Or()
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            spec.Where(b => b
                .AndGroup(g => g
                    .Or(r => r.IsActive) // ❌ Should start with Where, not Or
                    .Or(r => r.Price > 100m))
            );
        });

        Assert.Contains("Groups must start with Where()", exception.Message);
    }

    [Fact]
    public void Group_StartingWithWhere_Succeeds()
    {
        // Arrange
        var spec = new FluentSpecificationBuilder<TestProduct>();

        // Act & Assert
        // This should succeed because groups start with Where()
        var builtSpec = spec.Where(b => b
            .AndGroup(g => g
                .Where(r => r.IsActive) // ✅ Correct: starts with Where
                .Or(r => r.Price > 100m))
        )
        .Build();

        Assert.NotNull(builtSpec);
        Assert.NotNull(builtSpec.Criteria);
    }

    [Fact]
    public void OrGroup_StartingWithWhere_Succeeds()
    {
        // Arrange
        var spec = new FluentSpecificationBuilder<TestProduct>();

        // Act & Assert
        // This should succeed because groups start with Where()
        var builtSpec = spec.Where(b => b
            .OrGroup(g => g
                .Where(r => r.IsActive) // ✅ Correct: starts with Where
                .Or(r => r.Price > 100m))
        )
        .Build();

        Assert.NotNull(builtSpec);
        Assert.NotNull(builtSpec.Criteria);
    }

    #endregion

    #region Nested Groups Tests

    [Fact]
    public async Task FindAsync_WithNestedGroupsStartingWithWhere_ReturnsCorrectResults()
    {
        // Arrange
        await SeedTestData();

        // Pattern with nested groups:
        // Where(b => b
        //     .And(...)
        //     .AndGroup(g1 => g1
        //         .Where(...)
        //         .Or(...)
        //         .AndGroup(g2 => g2
        //             .Where(...)
        //             .Or(...)
        //         )
        //     )
        // )
        // 
        // LINQ Equivalent:
        // var query = entities.Where(b => 
        //     condition1 &&
        //     (condition2 || condition3 || (condition4 || condition5))
        // );
        var spec = new FluentSpecificationBuilder<TestProduct>()
            .Where(b => b
                .And(r => r.Category == "Electronics")
                .AndGroup(g1 => g1
                    .Where(r => r.IsActive)
                    .Or(r => r.Price > 100m)
                    .AndGroup(g2 => g2
                        .Where(r => r.Price < 50m)
                        .Or(r => r.Name.Contains("Mouse")))
                )
            )
            .Build();

        // Act
        var results = await _repository.FindAsync(spec);
        var resultsList = results.ToList();

        // Assert
        Assert.NotEmpty(resultsList);
        Assert.All(resultsList, p =>
        {
            Assert.Equal("Electronics", p.Category);
            // Must satisfy: IsActive OR Price > 100 OR (Price < 50 OR Name contains "Mouse")
            var satisfiesOuterGroup = p.IsActive || p.Price > 100m || (p.Price < 50m || p.Name.Contains("Mouse"));
            Assert.True(satisfiesOuterGroup);
        });
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

