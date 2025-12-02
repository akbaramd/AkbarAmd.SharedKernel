using AkbarAmd.SharedKernel.Domain;
using AkbarAmd.SharedKernel.Domain.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;
using AkbarAmd.SharedKernel.Domain.Exceptions;

namespace MCA.SharedKernel.Domain.Test.BusinessRules;

/// <summary>
/// Tests for Business Rules usage in ValueObject base class
/// </summary>
public sealed class ValueObjectBusinessRuleTests
{
    [Fact]
    public void ValueObject_CheckRule_WhenRuleIsSatisfied_DoesNotThrow()
    {
        // Arrange
        var valueObject = new TestValueObject("valid@email.com");

        // Act & Assert
        var exception = Record.Exception(() => valueObject.ValidateEmail("valid@email.com"));
        Assert.Null(exception);
    }

    [Fact]
    public void ValueObject_CheckRule_WhenRuleIsNotSatisfied_ThrowsDomainBusinessRuleValidationException()
    {
        // Arrange
        var valueObject = new TestValueObject("invalid-email");

        // Act & Assert
        var exception = Assert.Throws<DomainBusinessRuleValidationException>(() => 
            valueObject.ValidateEmail("invalid-email"));
        
        Assert.NotNull(exception);
        Assert.NotNull(exception.BrokenRule);
    }

    [Fact]
    public void ValueObject_CheckRule_WhenRuleIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var valueObject = new TestValueObject("test@email.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            valueObject.ValidateEmailWithNullRule());
    }

    #region Test Helpers

    private class TestValueObject : ValueObject
    {
        private string _email;

        public TestValueObject(string email)
        {
            _email = email;
        }

        public void ValidateEmail(string email)
        {
            CheckRule(new EmailFormatRule(email), this);
            _email = email;
        }

        public void ValidateEmailWithNullRule()
        {
            IBusinessRule<TestValueObject>? nullRule = null;
            CheckRule(nullRule!, this);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _email;
        }
    }

    private class EmailFormatRule : BaseBusinessRule<TestValueObject>
    {
        private readonly string _email;

        public EmailFormatRule(string email)
        {
            _email = email;
        }

        public override bool IsSatisfiedBy(TestValueObject entity)
        {
            return !string.IsNullOrWhiteSpace(_email) && _email.Contains('@');
        }

        public override string Message => $"Email must be in valid format, but was '{_email}'";
    }

    #endregion
}

