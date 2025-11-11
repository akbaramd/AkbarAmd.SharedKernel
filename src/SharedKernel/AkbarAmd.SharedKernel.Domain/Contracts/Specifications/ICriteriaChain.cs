using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Fluent interface for chaining criteria operations (AND, OR, NOT, Group, OrGroup).
/// Returned by Where() method to enable fluent API.
/// </summary>
public interface ICriteriaChain<T>
{
    /// <summary>
    /// Adds an AND condition to the criteria chain.
    /// </summary>
    ICriteriaChain<T> And(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Adds an OR condition to the criteria chain.
    /// </summary>
    ICriteriaChain<T> Or(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Adds a NOT condition to the criteria chain.
    /// </summary>
    ICriteriaChain<T> Not(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Groups multiple conditions with AND and combines the group with the current root using AND.
    /// Example: Group(g => g.And(A).And(B)) creates (A AND B) AND (existing root)
    /// </summary>
    ICriteriaChain<T> Group(Func<ICriteriaChain<T>, ICriteriaChain<T>> group);

    /// <summary>
    /// Groups multiple conditions with AND and combines the group with the current root using OR.
    /// Example: OrGroup(g => g.And(A).And(B)) creates (A AND B) OR (existing root)
    /// </summary>
    ICriteriaChain<T> OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group);
}

