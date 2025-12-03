using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Fluent interface for Where(builder) context.
/// Does NOT allow Where() to be called inside - only And, Or, Not, Group, OrGroup.
/// Groups inside can call Where() to start their chain.
/// </summary>
/// <typeparam name="T">The entity type for the specification.</typeparam>
public interface IWhereBuilder<T>
{
    /// <summary>
    /// Adds an AND condition to the criteria chain.
    /// </summary>
    IWhereBuilder<T> And(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Adds an OR condition to the criteria chain.
    /// </summary>
    IWhereBuilder<T> Or(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Groups multiple conditions and combines the group with the current root using AND.
    /// Groups must start with Where(), then can use And() or Or().
    /// Example: AndGroup(g => g.Where(A).And(B).Or(C)) creates (A AND B OR C) AND (existing root)
    /// </summary>
    IWhereBuilder<T> AndGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group);

    /// <summary>
    /// Groups multiple conditions and combines the group with the current root using OR.
    /// Groups must start with Where(), then can use And() or Or().
    /// Example: OrGroup(g => g.Where(A).And(B).Or(C)) creates (A AND B OR C) OR (existing root)
    /// </summary>
    IWhereBuilder<T> OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group);
}

