using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Fluent builder for constructing complex criteria expressions.
/// All criteria chains must start with Where() - And/Or/Group can only be used after Where().
/// </summary>
public sealed class CriteriaBuilder<T>
{
    private ICriteriaNode<T>? _root;

    /// <summary>
    /// Starts a criteria chain with an initial expression.
    /// This is the required entry point - all criteria must start with Where().
    /// After Where(), you can chain And(), Or(), AndGroup(), OrGroup().
    /// Use ! operator in expressions for negation: .And(p => !p.IsDeleted)
    /// </summary>
    /// <example>
    /// <code>
    /// var builder = new CriteriaBuilder&lt;Product&gt;();
    /// var expr = builder
    ///     .Where(p => p.IsActive)
    ///     .And(p => p.Price > 100m)
    ///     .Or(p => p.Category == "Electronics")
    ///     .And(p => !p.IsDeleted)
    ///     .Build();
    /// </code>
    /// </example>
    public IWhereChain<T> Where(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        var node = new PredicateNode<T>(expr);
        _root = node;
        return new WhereChain(this);
    }

    /// <summary>
    /// Builds the final expression from the criteria tree.
    /// Returns null if no criteria were added (Where() was never called).
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

    /// <summary>
    /// Internal implementation of IWhereChain for CriteriaBuilder.
    /// </summary>
    private sealed class WhereChain : IWhereChain<T>
    {
        private readonly CriteriaBuilder<T> _builder;

        public WhereChain(CriteriaBuilder<T> builder)
        {
            _builder = builder;
        }

        public IWhereChain<T> Where(Expression<Func<T, bool>> expr)
        {
            // Where() can be called again to start a new chain (replaces root)
            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            var node = new PredicateNode<T>(expr);
            _builder._root = node;
            return this;
        }

        public IWhereChain<T> And(Expression<Func<T, bool>> expr)
        {
            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            var node = new PredicateNode<T>(expr);
            _builder._root = _builder._root is null 
                ? node 
                : new AndNode<T>(_builder._root, node);
            return this;
        }

        public IWhereChain<T> Or(Expression<Func<T, bool>> expr)
        {
            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            var node = new PredicateNode<T>(expr);
            _builder._root = _builder._root is null 
                ? node 
                : new OrNode<T>(_builder._root, node);
            return this;
        }


        public IWhereChain<T> AndGroup(Func<IWhereChain<T>, IWhereChain<T>> group)
        {
            if (group is null)
                throw new ArgumentNullException(nameof(group));

            // Create a group builder that will be used inside the group function
            // The group function must call Where() first on the provided chain
            var groupBuilder = new CriteriaBuilder<T>();
            var groupChain = new GroupWhereChain(groupBuilder);
            var built = group(groupChain);

            // Get the root from the group builder
            var builtGroup = groupBuilder._root;
            if (builtGroup is null)
                throw new InvalidOperationException("AndGroup cannot be empty. Groups must start with Where().");

            _builder._root = _builder._root is null 
                ? builtGroup 
                : new AndNode<T>(_builder._root, builtGroup);
            return this;
        }

        public IWhereChain<T> OrGroup(Func<IWhereChain<T>, IWhereChain<T>> group)
        {
            if (group is null)
                throw new ArgumentNullException(nameof(group));

            // Create a group builder that will be used inside the group function
            // The group function must call Where() first on the provided chain
            var groupBuilder = new CriteriaBuilder<T>();
            var groupChain = new GroupWhereChain(groupBuilder);
            var built = group(groupChain);

            // Get the root from the group builder
            var builtGroup = groupBuilder._root;
            if (builtGroup is null)
                throw new InvalidOperationException("Group cannot be empty. Groups must start with Where().");

            _builder._root = _builder._root is null 
                ? builtGroup 
                : new OrNode<T>(_builder._root, builtGroup);
            return this;
        }
    }

    /// <summary>
    /// Special implementation of IWhereChain for groups that enforces Where() first.
    /// </summary>
    private sealed class GroupWhereChain : IWhereChain<T>
    {
        private readonly CriteriaBuilder<T> _builder;
        private bool _hasWhere;

        public GroupWhereChain(CriteriaBuilder<T> builder)
        {
            _builder = builder;
        }

        public IWhereChain<T> Where(Expression<Func<T, bool>> expr)
        {
            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            if (_hasWhere)
                throw new InvalidOperationException("Where() can only be called once at the start of a group.");

            var node = new PredicateNode<T>(expr);
            _builder._root = node;
            _hasWhere = true;
            return this;
        }

        public IWhereChain<T> And(Expression<Func<T, bool>> expr)
        {
            if (!_hasWhere)
                throw new InvalidOperationException("Groups must start with Where(). Use AndGroup(g => g.Where(...).And(...))");

            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            var node = new PredicateNode<T>(expr);
            _builder._root = _builder._root is null 
                ? node 
                : new AndNode<T>(_builder._root, node);
            return this;
        }

        public IWhereChain<T> Or(Expression<Func<T, bool>> expr)
        {
            if (!_hasWhere)
                throw new InvalidOperationException("Groups must start with Where(). Use Group(g => g.Where(...).Or(...))");

            if (expr is null)
                throw new ArgumentNullException(nameof(expr));

            var node = new PredicateNode<T>(expr);
            _builder._root = _builder._root is null 
                ? node 
                : new OrNode<T>(_builder._root, node);
            return this;
        }


        public IWhereChain<T> AndGroup(Func<IWhereChain<T>, IWhereChain<T>> group)
        {
            if (!_hasWhere)
                throw new InvalidOperationException("Groups must start with Where(). Use AndGroup(g => g.Where(...).AndGroup(...))");

            if (group is null)
                throw new ArgumentNullException(nameof(group));

            var groupBuilder = new CriteriaBuilder<T>();
            var groupChain = new GroupWhereChain(groupBuilder);
            var built = group(groupChain);

            var builtGroup = groupBuilder._root;
            if (builtGroup is null)
                throw new InvalidOperationException("Nested AndGroup cannot be empty. Groups must start with Where().");

            _builder._root = _builder._root is null 
                ? builtGroup 
                : new AndNode<T>(_builder._root, builtGroup);
            return this;
        }

        public IWhereChain<T> OrGroup(Func<IWhereChain<T>, IWhereChain<T>> group)
        {
            if (!_hasWhere)
                throw new InvalidOperationException("Groups must start with Where(). Use AndGroup(g => g.Where(...).OrGroup(...))");

            if (group is null)
                throw new ArgumentNullException(nameof(group));

            var groupBuilder = new CriteriaBuilder<T>();
            var groupChain = new GroupWhereChain(groupBuilder);
            var built = group(groupChain);

            var builtGroup = groupBuilder._root;
            if (builtGroup is null)
                throw new InvalidOperationException("Nested group cannot be empty. Groups must start with Where().");

            _builder._root = _builder._root is null 
                ? builtGroup 
                : new OrNode<T>(_builder._root, builtGroup);
            return this;
        }
    }
}

