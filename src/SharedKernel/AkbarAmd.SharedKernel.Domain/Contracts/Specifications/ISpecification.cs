using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Defines a domain specification that expresses business rules and criteria.
/// Pure domain specification - contains only filtering criteria, no infrastructure concerns.
/// Includes, sorting, and pagination are handled at the repository level.
/// 
/// This interface aligns with the classic DDD Specification pattern (Evans/Fowler):
/// - IsSatisfiedBy: for in-memory domain validation and checks
/// - ToExpression: for repository/query use with EF Core or other ORMs
/// </summary>
public interface ISpecification<T>
{
    /// <summary>
    /// Gets the criteria expression that defines the business rule or filter.
    /// This is the primary property for repository/query use.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Determines whether a candidate object satisfies the specification.
    /// This is the classic DDD method for in-memory validation and domain checks.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate satisfies the specification; otherwise, false.</returns>
    bool IsSatisfiedBy(T candidate);

    /// <summary>
    /// Converts the specification to an expression tree for use in queries.
    /// This is an alias for the Criteria property, provided for clarity and API consistency.
    /// </summary>
    /// <returns>The criteria expression, or null if no criteria are defined.</returns>
    Expression<Func<T, bool>>? ToExpression();
}