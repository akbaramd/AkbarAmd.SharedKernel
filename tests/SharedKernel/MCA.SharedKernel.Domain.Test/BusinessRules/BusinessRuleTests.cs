using AkbarAmd.SharedKernel.Domain.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Exceptions;

namespace MCA.SharedKernel.Domain.Test.BusinessRules;

/// <summary>
/// Comprehensive tests for Business Rules functionality
/// </summary>
public sealed class BusinessRuleTests
{
    #region IBusinessRule Interface Tests

    [Fact]
    public void IBusinessRule_IsSatisfiedBy_WhenRuleIsSatisfied_ReturnsTrue()
    {
        // Arrange
        IBusinessRule<TestEntity> rule = new PositiveValueRule(10);

        // Act
        var result = rule.IsSatisfiedBy(new TestEntity { Value = 10 });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IBusinessRule_IsSatisfiedBy_WhenRuleIsNotSatisfied_ReturnsFalse()
    {
        // Arrange
        IBusinessRule<TestEntity> rule = new PositiveValueRule(-5);

        // Act
        var result = rule.IsSatisfiedBy(new TestEntity { Value = -5 });

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IBusinessRule_Message_ReturnsMeaningfulMessage()
    {
        // Arrange
        IBusinessRule<TestEntity> rule = new PositiveValueRule(0);

        // Act
        var message = rule.Message;

        // Assert
        Assert.NotNull(message);
        Assert.NotEmpty(message);
    }

    #endregion

    #region BaseBusinessRule Tests

    [Fact]
    public void BaseBusinessRule_IsSatisfiedBy_WhenRuleIsSatisfied_ReturnsTrue()
    {
        // Arrange
        var rule = new PositiveValueRule(10);
        var entity = new TestEntity { Value = 10 };

        // Act
        var result = rule.IsSatisfiedBy(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void BaseBusinessRule_IsSatisfiedBy_WhenRuleIsNotSatisfied_ReturnsFalse()
    {
        // Arrange
        var rule = new PositiveValueRule(-5);
        var entity = new TestEntity { Value = -5 };

        // Act
        var result = rule.IsSatisfiedBy(entity);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BaseBusinessRule_ToString_ReturnsFormattedString()
    {
        // Arrange
        var rule = new PositiveValueRule(0);

        // Act
        var result = rule.ToString();

        // Assert
        Assert.Contains("PositiveValueRule", result);
        Assert.Contains(rule.Message, result);
    }

    [Fact]
    public void BaseBusinessRule_Message_IsAccessible()
    {
        // Arrange
        var rule = new PositiveValueRule(0);

        // Act
        var message = rule.Message;

        // Assert
        Assert.NotNull(message);
        Assert.NotEmpty(message);
    }

    #endregion

    #region DomainBusinessRuleValidationException Tests

    [Fact]
    public void DomainBusinessRuleValidationException_Create_WhenRuleIsNull_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            DomainBusinessRuleValidationException.Create<TestEntity>(null!));
    }

    [Fact]
    public void DomainBusinessRuleValidationException_Create_WhenRuleIsViolated_CreatesExceptionWithMessage()
    {
        // Arrange
        var rule = new PositiveValueRule(-5);

        // Act
        var exception = DomainBusinessRuleValidationException.Create(rule);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(rule.Message, exception.Message);
        Assert.NotNull(exception.BrokenRule);
    }

    [Fact]
    public void DomainBusinessRuleValidationException_ToString_ReturnsFormattedString()
    {
        // Arrange
        var rule = new PositiveValueRule(-5);
        var exception = DomainBusinessRuleValidationException.Create(rule);

        // Act
        var result = exception.ToString();

        // Assert
        Assert.Contains("PositiveValueRule", result);
        Assert.Contains(rule.Message, result);
    }

    [Fact]
    public void DomainBusinessRuleValidationException_BrokenRule_IsSet()
    {
        // Arrange
        var rule = new PositiveValueRule(-5);

        // Act
        var exception = DomainBusinessRuleValidationException.Create(rule);

        // Assert
        Assert.NotNull(exception.BrokenRule);
        Assert.Equal(rule, exception.BrokenRule);
    }

    #endregion

    #region Test Helpers

    private class TestEntity
    {
        public int Value { get; set; }
    }

    private class PositiveValueRule : BaseBusinessRule<TestEntity>
    {
        private readonly int _value;

        public PositiveValueRule(int value)
        {
            _value = value;
        }

        public override bool IsSatisfiedBy(TestEntity entity)
        {
            return entity.Value > 0;
        }

        public override string Message => $"Value must be positive, but was {_value}";
    }

    #endregion
}

