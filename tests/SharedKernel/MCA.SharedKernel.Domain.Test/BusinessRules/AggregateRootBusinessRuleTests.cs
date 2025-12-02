using AkbarAmd.SharedKernel.Domain.AggregateRoots;
using AkbarAmd.SharedKernel.Domain.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Exceptions;

namespace MCA.SharedKernel.Domain.Test.BusinessRules;

/// <summary>
/// Tests for Business Rules usage in AggregateRoot base class
/// </summary>
public sealed class AggregateRootBusinessRuleTests
{
    [Fact]
    public void AggregateRoot_CheckRule_WhenRuleIsSatisfied_DoesNotThrow()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid(), "Test Name");

        // Act & Assert
        var exception = Record.Exception(() => aggregate.UpdateName("New Name"));
        Assert.Null(exception);
    }

    [Fact]
    public void AggregateRoot_CheckRule_WhenRuleIsNotSatisfied_ThrowsDomainBusinessRuleValidationException()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid(), "Test Name");

        // Act & Assert
        var exception = Assert.Throws<DomainBusinessRuleValidationException>(() => 
            aggregate.UpdateName(""));
        
        Assert.NotNull(exception);
        Assert.NotNull(exception.BrokenRule);
    }

    [Fact]
    public void AggregateRoot_CheckRule_InConstructor_WhenRuleIsSatisfied_CreatesAggregate()
    {
        // Arrange & Act
        var aggregate = new TestAggregate(Guid.NewGuid(), "Valid Name");

        // Assert
        Assert.NotNull(aggregate);
        Assert.Equal("Valid Name", aggregate.Name);
    }

    [Fact]
    public void AggregateRoot_CheckRule_InConstructor_WhenRuleIsNotSatisfied_ThrowsDomainBusinessRuleValidationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainBusinessRuleValidationException>(() => 
            new TestAggregate(Guid.NewGuid(), ""));
    }

    [Fact]
    public void AggregateRoot_CheckRule_MultipleRules_WhenAllSatisfied_DoesNotThrow()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid(), "Valid Name");

        // Act & Assert
        var exception = Record.Exception(() => aggregate.UpdateNameAndValue("New Name", 100));
        Assert.Null(exception);
    }

    [Fact]
    public void AggregateRoot_CheckRule_MultipleRules_WhenOneFails_ThrowsDomainBusinessRuleValidationException()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid(), "Valid Name");

        // Act & Assert
        var exception = Assert.Throws<DomainBusinessRuleValidationException>(() => 
            aggregate.UpdateNameAndValue("", 100));
        
        Assert.NotNull(exception);
        Assert.Contains("Name", exception.Message);
    }

    #region Test Helpers

    private class TestAggregate : AggregateRoot<Guid>
    {
        public string Name { get; private set; }
        public int Value { get; private set; }

        public TestAggregate(Guid id, string name) : base(id)
        {
            CheckRule(new NameCannotBeEmptyRule(name), this);
            Name = name;
            Value = 0;
        }

        public void UpdateName(string newName)
        {
            CheckRule(new NameCannotBeEmptyRule(newName), this);
            Name = newName;
        }

        public void UpdateNameAndValue(string newName, int newValue)
        {
            CheckRule(new NameCannotBeEmptyRule(newName), this);
            CheckRule(new ValueMustBePositiveRule(newValue), this);
            Name = newName;
            Value = newValue;
        }
    }

    private class NameCannotBeEmptyRule : BaseBusinessRule<TestAggregate>
    {
        private readonly string _name;

        public NameCannotBeEmptyRule(string name)
        {
            _name = name;
        }

        public override bool IsSatisfiedBy(TestAggregate entity)
        {
            return !string.IsNullOrWhiteSpace(_name);
        }

        public override string Message => $"Name cannot be empty";
    }

    private class ValueMustBePositiveRule : BaseBusinessRule<TestAggregate>
    {
        private readonly int _value;

        public ValueMustBePositiveRule(int value)
        {
            _value = value;
        }

        public override bool IsSatisfiedBy(TestAggregate entity)
        {
            return _value > 0;
        }

        public override string Message => $"Value must be positive, but was {_value}";
    }

    #endregion
}

