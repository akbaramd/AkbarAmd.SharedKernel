/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Exceptions
 * Exception thrown when a business rule is violated.
 * Year: 2025
 */

using MCA.SharedKernel.Domain.Contracts;

namespace MCA.SharedKernel.Domain.Exceptions
{
    public class DomainBusinessRuleValidationException : Exception
    {
        public IBusinessRule BrokenRule { get; }

        public DomainBusinessRuleValidationException(IBusinessRule brokenRule)
            : base(brokenRule.Message)
        {
            BrokenRule = brokenRule;
        }

        public override string ToString()
        {
            return $"{BrokenRule.GetType().Name}: {BrokenRule.Message}";
        }
    }
}