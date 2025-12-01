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
    public void And_SingleExpression_ReturnsExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder.And(p => p.IsActive).Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false)));
    }

    [Fact]
    public void And_MultipleExpressions_CombinesWithAnd()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder
            .And(p => p.IsActive)
            .And(p => p.Price > 50m)
            .Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false))); // Not active
        Assert.False(compiled(new TestProduct("Test", 30m, "Cat", isActive: true))); // Price too low
    }

    [Fact]
    public void Or_SingleExpression_ReturnsExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder.Or(p => p.IsActive).Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false)));
    }

    [Fact]
    public void Or_MultipleExpressions_CombinesWithOr()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder
            .Or(p => p.IsActive)
            .Or(p => p.Price > 200m)
            .Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: true))); // Active
        Assert.True(compiled(new TestProduct("Test", 300m, "Cat", isActive: false))); // High price
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: false))); // Neither
    }

    [Fact]
    public void Not_SingleExpression_ReturnsNegatedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder.Not(p => p.IsActive).Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.False(compiled(new TestProduct("Test", 100m, "Cat", isActive: true)));
        Assert.True(compiled(new TestProduct("Test", 100m, "Cat", isActive: false)));
    }

    [Fact]
    public void Not_WithAnd_CombinesCorrectly()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act
        var result = builder
            .And(p => p.Price > 50m)
            .Not(p => p.IsActive)
            .Build();

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
    public void Group_WithAndConditions_CreatesGroupedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: (A AND B) AND (C AND D)
        var result = builder
            .Group(g => g
                .And(p => p.IsActive)
                .And(p => p.Price > 50m))
            .Group(g => g
                .And(p => p.Category == "Electronics")
                .And(p => p.Price < 200m))
            .Build();

        // Assert
        Assert.NotNull(result);
        var compiled = result.Compile();
        Assert.True(compiled(new TestProduct("Test", 100m, "Electronics", isActive: true)));
        Assert.False(compiled(new TestProduct("Test", 100m, "Electronics", isActive: false))); // Not active
        Assert.False(compiled(new TestProduct("Test", 100m, "Furniture", isActive: true))); // Wrong category
    }

    [Fact]
    public void OrGroup_WithConditions_CreatesOrGroupedExpression()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: (A AND B) OR (C AND D)
        var result = builder
            .Group(g => g
                .And(p => p.IsActive)
                .And(p => p.Price > 50m))
            .OrGroup(g => g
                .And(p => p.Category == "Electronics")
                .And(p => p.Price < 200m))
            .Build();

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
            builder.Group(g => g).Build());
    }

    [Fact]
    public void OrGroup_EmptyGroup_ThrowsException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            builder.OrGroup(g => g).Build());
    }

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void ComplexExpression_WithNestedGroups_WorksCorrectly()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act: ((A AND B) OR (C AND NOT D)) AND E
        var result = builder
            .Group(g => g
                .Group(inner => inner
                    .And(p => p.IsActive)
                    .And(p => p.Price > 50m))
                .OrGroup(inner => inner
                    .And(p => p.Category == "Electronics")
                    .Not(p => p.IsActive)))
            .And(p => p.Price < 500m)
            .Build();

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
    public void And_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.And(null!));
    }

    [Fact]
    public void Or_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Or(null!));
    }

    [Fact]
    public void Not_NullExpression_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Not(null!));
    }

    [Fact]
    public void Group_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.Group(null!));
    }

    [Fact]
    public void OrGroup_NullFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = new CriteriaBuilder<TestProduct>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            builder.OrGroup(null!));
    }

    #endregion
}

