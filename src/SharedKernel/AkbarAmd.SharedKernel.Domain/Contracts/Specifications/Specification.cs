using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Base implementation of ISpecification that provides common functionality
/// for building domain specifications with criteria only.
/// Supports both legacy AddCriteria (AND-based) and new Where() fluent API for complex criteria.
/// Pure domain specification - no infrastructure concerns (includes, sorting, pagination).
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    // Legacy AND-based criteria list for backward compatibility
    private readonly List<Expression<Func<T, bool>>> _criteriaAnd = new();

    // Tree-based criteria for Where() fluent API
    internal ICriteriaNode<T>? CriteriaTree { get; private set; }

    /// <summary>
    /// Gets the criteria expression that defines the business rule or filter.
    /// Combines legacy AddCriteria expressions with Where() fluent API expressions.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria
    {
        get
        {
            var fromList = _criteriaAnd.Count switch
            {
                0 => null,
                1 => _criteriaAnd[0],
                _ => _criteriaAnd.Aggregate(AndAlso)
            };

            var fromTree = BuildTreeExpression();
            return CombineAnd(fromList, fromTree);
        }
    }

    /// <summary>
    /// Determines whether a candidate object satisfies the specification.
    /// This is the classic DDD method for in-memory validation and domain checks.
    /// Compiles the criteria expression and evaluates it against the candidate.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate satisfies the specification; otherwise, false.</returns>
    public bool IsSatisfiedBy(T candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        var criteria = Criteria;
        if (criteria is null)
            return true; // No criteria means all candidates satisfy the spec

        // Compile and evaluate the expression
        var compiled = criteria.Compile();
        return compiled(candidate);
    }

    /// <summary>
    /// Converts the specification to an expression tree for use in queries.
    /// This is an alias for the Criteria property, provided for clarity and API consistency.
    /// </summary>
    /// <returns>The criteria expression, or null if no criteria are defined.</returns>
    public Expression<Func<T, bool>>? ToExpression() => Criteria;

    /// <summary>
    /// Legacy API: Adds a criteria expression that will be combined with AND.
    /// This is backward-compatible. For complex criteria with OR/NOT, use the Where() method.
    /// </summary>
    protected void AddCriteria(Expression<Func<T, bool>> criteriaExpression)
    {
        if (criteriaExpression is null)
            throw new ArgumentNullException(nameof(criteriaExpression));
        _criteriaAnd.Add(criteriaExpression);
    }

    /// <summary>
    /// Entry point for fluent API: Starts a criteria chain with an initial expression.
    /// Example: Where(p => p.IsActive).And(p => p.Category == "Electronics").Or(p => p.Price > 100)
    /// </summary>
    protected ICriteriaChain<T> Where(Expression<Func<T, bool>> start)
    {
        if (start is null)
            throw new ArgumentNullException(nameof(start));
        return CriteriaChain<T>.StartAttached(this, start, combineAsOr: false);
    }

    /// <summary>
    /// Entry point for fluent API: Starts a criteria chain using a builder function.
    /// The builder can start with And(), Or() - but NOT Where().
    /// Use ! operator in expressions for negation: .And(p => !p.IsDeleted)
    /// Groups inside can call Where() to start their chain.
    /// Example: Where(b => b.And(p => p.IsActive).Or(p => p.Stock > 0))
    /// Example: Where(b => b.And(p => !(p.Price == 0)).And(p => p.IsActive))
    /// Example: Where(b => b.AndGroup(g => g.Where(p => p.IsActive).And(p => p.Price > 100)))
    /// </summary>
    protected ICriteriaChain<T> Where(Func<IWhereBuilder<T>, IWhereBuilder<T>> builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        // Create an attached chain in builder context (not detached) so And/Or/Not can be used as first operations
        // But Where() is NOT allowed in this context
        var chain = CriteriaChain<T>.StartAttachedForBuilder(this);
        var built = builder(chain) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Where clause cannot be empty. At least one condition must be added.");

        // Root is already merged into the tree through the attached chain
        return CriteriaChain<T>.AttachOnExisting(this);
    }

    /// <summary>
    /// Merges a new subtree into the criteria tree.
    /// </summary>
    internal void MergeIntoTree(ICriteriaNode<T> newSubtree, bool combineAsOr)
    {
        if (newSubtree is null)
            throw new ArgumentNullException(nameof(newSubtree));

        CriteriaTree = CriteriaTree is null
            ? newSubtree
            : combineAsOr
                ? new OrNode<T>(CriteriaTree, newSubtree)
                : new AndNode<T>(CriteriaTree, newSubtree);
    }

    /// <summary>
    /// Replaces an old subtree with a new one in the criteria tree.
    /// Used when a chain's root is updated (e.g., when Or/And is called).
    /// </summary>
    internal void ReplaceInTree(ICriteriaNode<T> oldSubtree, ICriteriaNode<T> newSubtree)
    {
        if (oldSubtree is null)
            throw new ArgumentNullException(nameof(oldSubtree));
        if (newSubtree is null)
            throw new ArgumentNullException(nameof(newSubtree));

        if (CriteriaTree is null)
        {
            CriteriaTree = newSubtree;
            return;
        }

        // If the old subtree is the root, replace it
        if (ReferenceEquals(CriteriaTree, oldSubtree))
        {
            CriteriaTree = newSubtree;
            return;
        }

        // Otherwise, we need to find and replace it in the tree
        CriteriaTree = ReplaceNode(CriteriaTree, oldSubtree, newSubtree);
    }

    private ICriteriaNode<T> ReplaceNode(ICriteriaNode<T> current, ICriteriaNode<T> oldNode, ICriteriaNode<T> newNode)
    {
        if (ReferenceEquals(current, oldNode))
            return newNode;

        if (current is AndNode<T> andNode)
        {
            var newLeft = ReplaceNode(andNode.Left, oldNode, newNode);
            var newRight = ReplaceNode(andNode.Right, oldNode, newNode);
            if (ReferenceEquals(newLeft, andNode.Left) && ReferenceEquals(newRight, andNode.Right))
                return current;
            return new AndNode<T>(newLeft, newRight);
        }

        if (current is OrNode<T> orNode)
        {
            var newLeft = ReplaceNode(orNode.Left, oldNode, newNode);
            var newRight = ReplaceNode(orNode.Right, oldNode, newNode);
            if (ReferenceEquals(newLeft, orNode.Left) && ReferenceEquals(newRight, orNode.Right))
                return current;
            return new OrNode<T>(newLeft, newRight);
        }

        if (current is NotNode<T> notNode)
        {
            var newInner = ReplaceNode(notNode.Inner, oldNode, newNode);
            if (ReferenceEquals(newInner, notNode.Inner))
                return current;
            return new NotNode<T>(newInner);
        }

        return current;
    }

    // ---- Helpers

    private Expression<Func<T, bool>>? BuildTreeExpression()
    {
        if (CriteriaTree is null)
            return null;

        var param = Expression.Parameter(typeof(T), "x");
        var body = CriteriaTree.ToExpressionBody(param);
        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    // EF-safe AND combiner for legacy list
    private static Expression<Func<T, bool>> AndAlso(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left is null) return right;
        if (right is null) return left;

        var param = left.Parameters[0];
        var rewrittenRightBody = ParameterReplacer.Replace(right.Parameters[0], param, right.Body);
        var combined = Expression.AndAlso(left.Body, rewrittenRightBody);
        return Expression.Lambda<Func<T, bool>>(combined, param);
    }

    private static Expression<Func<T, bool>>? CombineAnd(
        Expression<Func<T, bool>>? a,
        Expression<Func<T, bool>>? b)
    {
        if (a is null) return b;
        if (b is null) return a;

        var param = a.Parameters[0];
        var bBody = ParameterReplacer.Replace(b.Parameters[0], param, b.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(a.Body, bBody), param);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        private ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from ?? throw new ArgumentNullException(nameof(from));
            _to = to ?? throw new ArgumentNullException(nameof(to));
        }

        public static Expression Replace(ParameterExpression from, ParameterExpression to, Expression body)
            => new ParameterReplacer(from, to).Visit(body)!;

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }
}
