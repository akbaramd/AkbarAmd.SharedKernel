/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Business Rule Interface
 * Represents a business rule that can be validated within domain logic.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;

/// <summary>
/// Defines a contract for a business rule within the domain.
/// Business rules encapsulate domain-specific validations or constraints
/// that must be satisfied to ensure the integrity of domain entities or operations.
/// 
/// This interface aligns with DDD principles:
/// - Encapsulates domain invariants and constraints
/// - Enforces business rules at the domain level
/// - Provides clear error messages for violations
/// - Used for fail-fast validation during entity construction and state transitions
/// </summary>
/// <typeparam name="TEntity">The type of entity this business rule validates</typeparam>
public interface IBusinessRule<in TEntity>
    where TEntity : class
{
    /// <summary>
    /// Evaluates the business rule against the specified entity.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <returns>
    /// <c>true</c> if the rule is satisfied; otherwise, <c>false</c>.
    /// </returns>
    bool IsSatisfiedBy(TEntity entity);

    /// <summary>
    /// Gets the message describing the rule or reason for failure.
    /// This message is intended to be user-friendly or useful for logging purposes.
    /// </summary>
    string Message { get; }
}

