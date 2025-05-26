/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Exceptions
 * Exception thrown when a business rule is violated.
 * Year: 2025
 */

using CleanArchitecture.Domain.SharedKernel.Rules;
using System;

namespace CleanArchitecture.Domain.SharedKernel.Exceptions
{
    public class BusinessRuleValidationException : Exception
    {
        public IBusinessRule BrokenRule { get; }

        public BusinessRuleValidationException(IBusinessRule brokenRule)
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