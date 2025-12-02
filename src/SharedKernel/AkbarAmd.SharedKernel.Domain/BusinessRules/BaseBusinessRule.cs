/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Business Rules
 * Base implementation for business rules following DDD principles.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts.BusinessRules;

namespace AkbarAmd.SharedKernel.Domain.BusinessRules;

/// <summary>
/// Base implementation of IBusinessRule that provides common functionality
/// for building domain business rules with consistent structure.
/// 
/// This base class follows DDD principles:
/// - Encapsulates domain invariants
/// - Provides consistent error message handling
/// - Supports immutable rule evaluation
/// - Aligns with SOLID principles (SRP, OCP)
/// </summary>
/// <typeparam name="TEntity">The type of entity this business rule validates</typeparam>
public abstract class BaseBusinessRule<TEntity> : IBusinessRule<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Evaluates the business rule against the specified entity.
    /// Derived classes must implement this method to provide the rule logic.
    /// </summary>
    /// <param name="entity">The entity to validate</param>
    /// <returns>
    /// <c>true</c> if the rule is satisfied; otherwise, <c>false</c>.
    /// </returns>
    public abstract bool IsSatisfiedBy(TEntity entity);

    /// <summary>
    /// Gets the message describing the rule or reason for failure.
    /// Derived classes must provide a meaningful error message.
    /// </summary>
    public abstract string Message { get; }

    /// <summary>
    /// Returns a string representation of the business rule.
    /// </summary>
    /// <returns>A string that represents the current business rule.</returns>
    public override string ToString() => $"{GetType().Name}: {Message}";
}

