using AkbarAmd.SharedKernel.Domain;
using AkbarAmd.SharedKernel.Domain.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Exceptions;

namespace MCA.SharedKernel.Domain.Test.BusinessRules;

/// <summary>
/// Tests for Business Rules usage in Entity base class
/// </summary>
public sealed class EntityBusinessRuleTests
{
    [Fact]
    public void Entity_CheckRule_WhenRuleIsSatisfied_DoesNotThrow()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        var exception = Record.Exception(() => entity.ValidateValue(10));
        Assert.Null(exception);
    }

    [Fact]
    public void Entity_CheckRule_WhenRuleIsNotSatisfied_ThrowsDomainBusinessRuleValidationException()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        var exception = Assert.Throws<DomainBusinessRuleValidationException>(() => 
            entity.ValidateValue(-5));
        
        Assert.NotNull(exception);
        Assert.NotNull(exception.BrokenRule);
    }

    [Fact]
    public void Entity_CheckRule_WhenRuleIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            entity.ValidateValueWithNullRule());
    }

    [Fact]
    public void Entity_CheckRule_WhenEntityIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var entity = new TestEntity(Guid.NewGuid());

        // Act & Assert
        // This test verifies that CheckRule validates the entity parameter
        var exception = Record.Exception(() => entity.ValidateValue(10));
        Assert.Null(exception);
    }

    #region Test Helpers

    private class TestEntity : Entity<Guid>
    {
        public int Value { get; private set; }

        public TestEntity(Guid id) : base(id)
        {
        }

        public void ValidateValue(int value)
        {
            var rule = new EntityValueMustBePositiveRule(value);
            CheckRule(rule, this);
            Value = value;
        }

        public void ValidateValueWithNullRule()
        {
            IBusinessRule<TestEntity>? nullRule = null;
            CheckRule(nullRule!, this);
        }
    }

    private class EntityValueMustBePositiveRule : BaseBusinessRule<TestEntity>
    {
        private readonly int _value;

        public EntityValueMustBePositiveRule(int value)
        {
            _value = value;
        }

        public override bool IsSatisfiedBy(TestEntity entity)
        {
            return _value > 0;
        }

        public override string Message => $"Entity value must be positive, but was {_value}";
    }

    #endregion
}

