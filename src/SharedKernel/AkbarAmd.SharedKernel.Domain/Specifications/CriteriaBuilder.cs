using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Fluent builder for constructing complex criteria expressions with AND, OR, NOT operations
/// and grouping support for parentheses.
/// </summary>
public sealed class CriteriaBuilder<T>
{
    private ICriteriaNode<T>? _root;

    /// <summary>
    /// Adds an AND condition to the criteria tree.
    /// If no root exists, this becomes the root. Otherwise, it's combined with AND.
    /// </summary>
    public CriteriaBuilder<T> And(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        var node = new PredicateNode<T>(expr);
        _root = _root is null ? node : new AndNode<T>(_root, node);
        return this;
    }

    /// <summary>
    /// Adds an OR condition to the criteria tree.
    /// If no root exists, this becomes the root. Otherwise, it's combined with OR.
    /// </summary>
    public CriteriaBuilder<T> Or(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        var node = new PredicateNode<T>(expr);
        _root = _root is null ? node : new OrNode<T>(_root, node);
        return this;
    }

    /// <summary>
    /// Adds a NOT condition to the criteria tree.
    /// If no root exists, this becomes the root. Otherwise, it's combined with AND.
    /// </summary>
    public CriteriaBuilder<T> Not(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        var node = new NotNode<T>(new PredicateNode<T>(expr));
        _root = _root is null ? node : new AndNode<T>(_root, node);
        return this;
    }

    /// <summary>
    /// Groups multiple conditions with AND and combines the group with the root using AND.
    /// Example: Group(g => g.And(A).And(B)) creates (A AND B) AND (existing root)
    /// </summary>
    public CriteriaBuilder<T> Group(Func<CriteriaBuilder<T>, CriteriaBuilder<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var groupBuilder = new CriteriaBuilder<T>();
        var builtGroup = group(groupBuilder)._root;

        if (builtGroup is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added.");

        _root = _root is null ? builtGroup : new AndNode<T>(_root, builtGroup);
        return this;
    }

    /// <summary>
    /// Groups multiple conditions with AND and combines the group with the root using OR.
    /// Example: OrGroup(g => g.And(A).And(B)) creates (A AND B) OR (existing root)
    /// </summary>
    public CriteriaBuilder<T> OrGroup(Func<CriteriaBuilder<T>, CriteriaBuilder<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var groupBuilder = new CriteriaBuilder<T>();
        var builtGroup = group(groupBuilder)._root;

        if (builtGroup is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added.");

        _root = _root is null ? builtGroup : new OrNode<T>(_root, builtGroup);
        return this;
    }

    /// <summary>
    /// Builds the final expression from the criteria tree.
    /// Returns null if no criteria were added.
    /// </summary>
    public Expression<Func<T, bool>>? Build()
    {
        if (_root is null)
            return null;

        var param = Expression.Parameter(typeof(T), "x");
        var body = _root.ToExpressionBody(param);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Internal method to get the root node without building the expression.
    /// Used by BaseSpecification for lazy evaluation.
    /// </summary>
    internal ICriteriaNode<T>? BuildNode() => _root;
}

