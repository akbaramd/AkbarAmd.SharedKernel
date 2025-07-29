/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Business Rule Interface
 * Represents a business rule that can be validated within domain logic.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Defines a contract for a business rule within the domain.
    /// Business rules encapsulate domain-specific validations or constraints
    /// that must be satisfied to ensure the integrity of domain entities or operations.
    /// </summary>
    public interface IBusinessRule
    {
        /// <summary>
        /// Evaluates the business rule.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the rule is satisfied; otherwise, <c>false</c>.
        /// </returns>
        bool IsSatisfied();

        /// <summary>
        /// Gets the message describing the rule or reason for failure.
        /// This message is intended to be user-friendly or useful for logging purposes.
        /// </summary>
        string Message { get; }
    }
}