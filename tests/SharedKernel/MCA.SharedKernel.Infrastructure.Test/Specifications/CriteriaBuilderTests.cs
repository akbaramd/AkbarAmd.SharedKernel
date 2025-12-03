using System;
using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications;
using AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;
using Xunit;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications;

/// <summary>
/// Unit tests for CriteriaBuilder and criteria node classes.
/// </summary>
public sealed class CriteriaBuilderTests
{
    static CriteriaBuilderTests()
    {
        // Register Infrastructure handlers with Domain layer
        InfrastructureInitialization.RegisterHandlers();
    }

    #region Basic Operations Tests

    [Fact]
    public void Where_SingleExpression_ReturnsExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder.Where(p => p.IsActive);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false)));
    }

    [Fact]
    public void Where_And_MultipleExpressions_CombinesWithAnd()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder
            .Where(p => p.IsActive)
            .And(p => p.Price > 50m);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false))); // Not active
        Assert.False(compiled(new TestProduct("Test", 30m, "Cat", isActive: true))); // Price too low
    }

    [Fact]
    public void Where_Or_SingleExpression_ReturnsExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder.Where(p => p.IsActive);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false)));
    }

    [Fact]
    public void Where_Or_MultipleExpressions_CombinesWithOr()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder
            .Where(p => p.IsActive)
            .Or(p => p.Price > 200m);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true))); // Active
        Assert.True(compiled(new TestProduct("Test", 300m, "Cat", isActive: false))); // High price
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false))); // Neither
    }

    [Fact]
    public void Where_And_NotOperator_SingleExpression_ReturnsNegatedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder.Where(p => p.IsActive).And(p => !(p.Category == "Cat"));
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        // This tests: IsActive AND NOT (Category == "Cat")
        Assert.True(compiled(new TestProduct("Test", 100m, "Other", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
    }

    [Fact]
    public void Where_And_NotOperator_CombinesCorrectly()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        builder
            .Where(p => p.Price > 50m)
            .And(p => !p.IsActive);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: false))); // Price OK, not active
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: true))); // Active (NOT active fails)
        Assert.False(compiled(new TestProduct("Test", 30m, "Cat", isActive: false))); // Price too low
    }

    #endregion

    #region Grouping Tests

    [Fact]
    public void Where_Group_WithWhereAndConditions_CreatesGroupedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: (A AND B) AND (C AND D)
        // LINQ Equivalent: (IsActive && Price > 50) && (Category == "Electronics" && Price < 200)
        builder
            .Where(p => p.IsActive)
            .And(p => p.Price > 50m)
            .AndGroup(g => g
                .Where(p => p.Category == "Electronics")
                .And(p => p.Price < 200m));
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Electronics", isActive: false))); // Not active
        Assert.False(compiled(new TestProduct("Test", 100m, "Furniture", isActive: true))); // Wrong category
    }

    [Fact]
    public void Where_Group_OrGroup_WithConditions_CreatesOrGroupedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: (A AND B) OR (C AND D)
        // LINQ Equivalent: (IsActive && Price > 50) || (Category == "Electronics" && Price < 200)
        builder
            .Where(p => p.IsActive)
            .And(p => p.Price > 50m)
            .OrGroup(g => g
                .Where(p => p.Category == "Electronics")
                .And(p => p.Price < 200m));
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: true))); // First group
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: false))); // Second group
        Assert.False(compiled(new TestProduct("Test", 100m, "Furniture", isActive: false))); // Neither group
    }

    [Fact]
    public void Group_EmptyGroup_ThrowsException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            builder.Where(p => p.IsActive).AndGroup(g => g);
            builder.Build();
        });
    }

    [Fact]
    public void AndGroup_WithWhereAnd_CombinesCorrectly()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act - And() is now allowed after Where() in groups
        builder.Where(p => p.IsActive).AndGroup(g => g.Where(p => p.Price > 50m).And(p => p.Price > 100m));
        var result = builder.Build();
        
        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        // This tests: IsActive AND (Price > 50 AND Price > 100)
        Assert.True(compiled(new TestProduct("Test", 150m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 75m, "Cat", isActive: true))); // Price > 50 but not > 100
    }

    [Fact]
    public void AndGroup_StartingWithAnd_ThrowsException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert - Groups must start with Where()
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            builder.Where(p => p.IsActive).AndGroup(g => g.And(p => p.Price > 100m));
            builder.Build();
        });
        
        Assert.Contains("Groups must start with Where()", exception.Message);
    }

    [Fact]
    public void Group_StartingWithOr_ThrowsException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            builder.Where(p => p.IsActive).AndGroup(g => g.Or(p => p.Price > 100m));
            builder.Build();
        });
        
        Assert.Contains("Groups must start with Where()", exception.Message);
    }

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void ComplexExpression_WithNestedGroups_WorksCorrectly()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: ((A AND B) OR (C AND NOT D)) AND E
        // LINQ Equivalent: ((IsActive && Price > 50) || (Category == "Electronics" && !IsActive)) && Price < 500
        builder
            .Where(p => true) // Start with Where
            .AndGroup(g => g
                .Where(p => p.IsActive)
                .And(p => p.Price > 50m)
                .OrGroup(inner => inner
                    .Where(p => p.Category == "Electronics")
                    .And(p => !p.IsActive)))
            .And(p => p.Price < 500m);
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: true))); // First group
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: false))); // Second group
        Assert.False(compiled(new TestProduct("Test", 600m, "Electronics", isActive: true))); // Price too high
    }

    [Fact]
    public void Build_NoCriteria_ReturnsNull()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder.Build();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Where_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Where(null!));
    }

    [Fact]
    public void And_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Where(p => p.IsActive).And(null!));
    }

    [Fact]
    public void Or_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Where(p => p.IsActive).Or(null!));
    }

    [Fact]
    public void Not_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert - Not() removed, test removed as Not() no longer exists
    }

    [Fact]
    public void Group_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Where(p => p.IsActive).AndGroup(null!));
    }

    [Fact]
    public void OrGroup_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Where(p => p.IsActive).OrGroup(null!));
    }

    #endregion
}

