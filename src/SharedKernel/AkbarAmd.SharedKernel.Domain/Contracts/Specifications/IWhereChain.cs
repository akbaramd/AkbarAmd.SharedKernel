using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Fluent interface returned by Where() method that allows chaining And, Or, AndGroup, OrGroup operations.
/// Enforces that all criteria chains must start with Where().
/// </summary>
/// <typeparam name="T">The entity type for the specification.</typeparam>
public interface IWhereChain<T>
{
    /// <summary>
    /// Starts a criteria chain with an initial expression (required for groups).
    /// Groups must start with Where() before using And/Or/Not.
    /// </summary>
    IWhereChain<T> Where(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Adds an AND condition to the criteria chain.
    /// Can only be used after Where().
    /// </summary>
    IWhereChain<T> And(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Adds an OR condition to the criteria chain.
    /// Can only be used after Where().
    /// </summary>
    IWhereChain<T> Or(Expression<Func<T, bool>> expr);

    /// <summary>
    /// Groups multiple conditions and combines the group with the current root using AND.
    /// Groups must start with Where(), then can use And() or Or().
    /// Example: AndGroup(g => g.Where(A).And(B).Or(C)) creates (A AND B OR C) AND (existing root)
    /// </summary>
    IWhereChain<T> AndGroup(Func<IWhereChain<T>, IWhereChain<T>> group);

    /// <summary>
    /// Groups multiple conditions and combines the group with the current root using OR.
    /// Groups must start with Where(), then can use And() or Or().
    /// Example: OrGroup(g => g.Where(A).And(B).Or(C)) creates (A AND B OR C) OR (existing root)
    /// </summary>
    IWhereChain<T> OrGroup(Func<IWhereChain<T>, IWhereChain<T>> group);
}

