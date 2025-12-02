/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Exceptions
 * Exception thrown when a business rule is violated.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;

namespace AkbarAmd.SharedKernel.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a domain business rule is violated.
    /// This exception encapsulates the broken rule to provide context about the violation.
    /// </summary>
    public class DomainBusinessRuleValidationException : Exception
    {
        /// <summary>
        /// Gets the business rule that was violated.
        /// </summary>
        public object BrokenRule { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainBusinessRuleValidationException"/> class.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity that violated the rule</typeparam>
        /// <param name="brokenRule">The business rule that was violated.</param>
        public static DomainBusinessRuleValidationException Create<TEntity>(IBusinessRule<TEntity> brokenRule)
            where TEntity : class
        {
            if (brokenRule == null)
                throw new ArgumentNullException(nameof(brokenRule));
                
            return new DomainBusinessRuleValidationException(brokenRule.Message, brokenRule);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainBusinessRuleValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="brokenRule">The business rule that was violated.</param>
        private DomainBusinessRuleValidationException(string message, object brokenRule)
            : base(message)
        {
            BrokenRule = brokenRule;
        }

        /// <summary>
        /// Returns a string representation of the exception, including the rule name and message.
        /// </summary>
        /// <returns>A string that represents the current exception.</returns>
        public override string ToString()
        {
            return $"{BrokenRule.GetType().Name}: {Message}";
        }
    }
}
