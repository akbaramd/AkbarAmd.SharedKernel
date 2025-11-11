using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Base implementation of ISpecification that provides common functionality
/// for building query specifications with criteria, includes, sorting, and paging.
/// Supports both legacy AddCriteria (AND-based) and new Where() fluent API for complex criteria.
/// Implements IMultiSortSpecification for multi-level sorting support.
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T>, IMultiSortSpecification<T>
{
    // Legacy AND-based criteria list for backward compatibility
    private readonly List<Expression<Func<T, bool>>> _criteriaAnd = new();

    // Tree-based criteria for Where() fluent API
    internal ICriteriaNode<T>? CriteriaTree { get; private set; }

    private readonly List<Expression<Func<T, object>>> _includes = new();
    private readonly List<string> _includeStrings = new();

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

    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;
    public IReadOnlyList<string> IncludeStrings => _includeStrings;

    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    // Multi-level sorting chain
    private readonly List<SortDescriptor<T>> _sorts = new();
    public IReadOnlyList<SortDescriptor<T>> Sorts => _sorts;

    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

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
    /// Example: Where(b => b.And(p => p.IsActive).Or(p => p.Stock > 0))
    /// </summary>
    protected ICriteriaChain<T> Where(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var detached = CriteriaChain<T>.StartDetached();
        var built = builder(detached) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid group builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Where clause cannot be empty. At least one condition must be added.");

        MergeIntoTree(built.Root, combineAsOr: false);
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

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        if (includeExpression is null)
            throw new ArgumentNullException(nameof(includeExpression));
        _includes.Add(includeExpression);
    }

    protected void AddInclude(string includeString)
    {
        if (string.IsNullOrWhiteSpace(includeString))
            throw new ArgumentException("Include cannot be empty.", nameof(includeString));
        _includeStrings.Add(includeString);
    }

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderBy = orderByExpression ?? throw new ArgumentNullException(nameof(orderByExpression));

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        => OrderByDescending = orderByDescExpression ?? throw new ArgumentNullException(nameof(orderByDescExpression));

    protected void ApplyPaging(int skip, int take)
    {
        if (skip < 0)  throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    // ======== New Fluent APIs ========

    /// <summary>
    /// Fluent API: Adds an include expression.
    /// </summary>
    public BaseSpecification<T> Include(Expression<Func<T, object>> include)
    {
        AddInclude(include);
        return this;
    }

    /// <summary>
    /// Fluent API: Adds an include by string path.
    /// </summary>
    public BaseSpecification<T> Include(string includePath)
    {
        AddInclude(includePath);
        return this;
    }

    /// <summary>
    /// Fluent API: Adds multiple include expressions.
    /// </summary>
    public BaseSpecification<T> Include(params Expression<Func<T, object>>[] includes)
    {
        if (includes is null)
            throw new ArgumentNullException(nameof(includes));
        foreach (var include in includes)
            AddInclude(include);
        return this;
    }

    /// <summary>
    /// Fluent API: Adds multiple include paths.
    /// </summary>
    public BaseSpecification<T> IncludePaths(params string[] paths)
    {
        if (paths is null)
            throw new ArgumentNullException(nameof(paths));
        foreach (var path in paths)
            AddInclude(path);
        return this;
    }

    /// <summary>
    /// Fluent API: Sets the primary sort key in ascending order.
    /// Clears any existing sort chain and starts a new one.
    /// </summary>
    public BaseSpecification<T> OrderByKey<TKey>(Expression<Func<T, TKey>> key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        _sorts.Clear(); // Start new chain
        _sorts.Add(new SortDescriptor<T>(key, SortDirection.Ascending, NullSort.Unspecified));
        
        // Sync with legacy properties
        OrderBy = ToObjectSelector(key);
        OrderByDescending = null;
        
        return this;
    }

    /// <summary>
    /// Fluent API: Sets the primary sort key in descending order.
    /// Clears any existing sort chain and starts a new one.
    /// </summary>
    public BaseSpecification<T> OrderByKeyDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        _sorts.Clear(); // Start new chain
        _sorts.Add(new SortDescriptor<T>(key, SortDirection.Descending, NullSort.Unspecified));
        
        // Sync with legacy properties
        OrderBy = null;
        OrderByDescending = ToObjectSelector(key);
        
        return this;
    }

    /// <summary>
    /// Fluent API: Adds a secondary sort key in ascending order.
    /// Requires that OrderByKey or OrderByKeyDescending has been called first.
    /// </summary>
    public BaseSpecification<T> ThenBy<TKey>(Expression<Func<T, TKey>> key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (_sorts.Count == 0)
            throw new InvalidOperationException("Call OrderByKey or OrderByKeyDescending first before using ThenBy.");

        _sorts.Add(new SortDescriptor<T>(key, SortDirection.Ascending, NullSort.Unspecified));
        return this;
    }

    /// <summary>
    /// Fluent API: Adds a secondary sort key in descending order.
    /// Requires that OrderByKey or OrderByKeyDescending has been called first.
    /// </summary>
    public BaseSpecification<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (_sorts.Count == 0)
            throw new InvalidOperationException("Call OrderByKey or OrderByKeyDescending first before using ThenByDescending.");

        _sorts.Add(new SortDescriptor<T>(key, SortDirection.Descending, NullSort.Unspecified));
        return this;
    }

    /// <summary>
    /// Fluent API: Sets null ordering policy to NullsFirst for the last sort descriptor.
    /// Only meaningful for nullable or reference types.
    /// </summary>
    public BaseSpecification<T> NullsFirst()
    {
        if (_sorts.Count == 0)
            throw new InvalidOperationException("Call OrderByKey/OrderByKeyDescending/ThenBy first before using NullsFirst.");

        var last = _sorts[^1];
        _sorts[^1] = last with { Nulls = NullSort.NullsFirst };
        return this;
    }

    /// <summary>
    /// Fluent API: Sets null ordering policy to NullsLast for the last sort descriptor.
    /// Only meaningful for nullable or reference types.
    /// </summary>
    public BaseSpecification<T> NullsLast()
    {
        if (_sorts.Count == 0)
            throw new InvalidOperationException("Call OrderByKey/OrderByKeyDescending/ThenBy first before using NullsLast.");

        var last = _sorts[^1];
        _sorts[^1] = last with { Nulls = NullSort.NullsLast };
        return this;
    }

    /// <summary>
    /// Fluent API: Sets pagination using page number and page size.
    /// Calculates skip and take automatically.
    /// </summary>
    public BaseSpecification<T> Page(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        return this;
    }

    /// <summary>
    /// Fluent API: Sets the skip value for pagination.
    /// </summary>
    public BaseSpecification<T> SkipBy(int skip)
    {
        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
        
        ApplyPaging(skip, Take > 0 ? Take : int.MaxValue);
        return this;
    }

    /// <summary>
    /// Fluent API: Sets the take value for pagination.
    /// </summary>
    public BaseSpecification<T> TakeBy(int take)
    {
        if (take <= 0)
            throw new ArgumentOutOfRangeException(nameof(take), "Take must be greater than zero.");
        
        ApplyPaging(Skip, take);
        return this;
    }

    // ---- Helpers

    /// <summary>
    /// Converts a typed lambda expression to an object selector for legacy compatibility.
    /// </summary>
    private static Expression<Func<T, object>> ToObjectSelector(LambdaExpression lambda)
    {
        var param = lambda.Parameters[0];
        var body = Expression.Convert(lambda.Body, typeof(object));
        return Expression.Lambda<Func<T, object>>(body, param);
    }

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
