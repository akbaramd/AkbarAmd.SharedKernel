using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Implementation of ICriteriaChain that manages criteria building with fluent API.
/// Supports both attached mode (connected to BaseSpecification) and detached mode (for groups).
/// </summary>
internal sealed class CriteriaChain<T> : ICriteriaChain<T>, IWhereBuilder<T>
{
    private readonly Specification<T>? _attachedSpec; // null => detached group chain
    private readonly bool _isDetachedGroup; // true => detached group that must start with Where
    private readonly bool _isBuilderContext; // true => Where(builder) context, cannot call Where() inside
    internal ICriteriaNode<T>? Root { get; private set; }

    private CriteriaChain(Specification<T>? attached, bool isDetachedGroup = false, bool isBuilderContext = false)
    {
        _attachedSpec = attached;
        _isDetachedGroup = isDetachedGroup;
        _isBuilderContext = isBuilderContext;
    }

    /// <summary>
    /// Starts a chain attached to a specification with an initial expression.
    /// </summary>
    public static CriteriaChain<T> StartAttached(Specification<T> spec, Expression<Func<T, bool>> start, bool combineAsOr)
    {
        if (start is null)
            throw new ArgumentNullException(nameof(start));
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        var chain = new CriteriaChain<T>(spec);
        var node = new PredicateNode<T>(start);
        chain.Root = node;
        spec.MergeIntoTree(node, combineAsOr);
        return chain;
    }

    /// <summary>
    /// Attaches to an existing specification's criteria tree.
    /// Used when Where(builder) is called without an initial expression.
    /// </summary>
    public static CriteriaChain<T> AttachOnExisting(Specification<T> spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        var chain = new CriteriaChain<T>(spec) { Root = spec.CriteriaTree };
        return chain;
    }

    /// <summary>
    /// Starts a detached chain for use in Group/OrGroup.
    /// Detached groups must start with Where().
    /// </summary>
    public static CriteriaChain<T> StartDetached() => new CriteriaChain<T>(attached: null, isDetachedGroup: true);

    /// <summary>
    /// Starts an attached chain for use in Where(builder) context.
    /// Allows And/Or/Not as first operations, but NOT Where().
    /// </summary>
    public static CriteriaChain<T> StartAttachedForBuilder(Specification<T> spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));
        return new CriteriaChain<T>(spec, isDetachedGroup: false, isBuilderContext: true);
    }

    public ICriteriaChain<T> Where(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        // Prevent Where() from being called inside Where(builder) context
        if (_isBuilderContext)
        {
            throw new InvalidOperationException(
                "Where() cannot be called inside Where(builder). Use And(), Or(), or AndGroup() instead. Use ! operator for negation. " +
                "Groups can call Where() to start their chain: AndGroup(g => g.Where(...).And(...))");
        }

        var node = new PredicateNode<T>(expr);
        var oldRoot = Root;
        Root = node; // Where always sets the root (or starts it)

        if (_attachedSpec is not null)
        {
            // If this is the first node in the chain, merge it
            // Otherwise, we need to update the spec's tree by removing old root and adding new root
            if (oldRoot is null)
            {
                _attachedSpec.MergeIntoTree(node, combineAsOr: false);
            }
            else
            {
                // Remove old root and add new root
                _attachedSpec.ReplaceInTree(oldRoot, Root);
            }
        }

        return this;
    }

    public ICriteriaChain<T> And(Expression<Func<T, bool>> expr) => AndImpl(expr);
    IWhereBuilder<T> IWhereBuilder<T>.And(Expression<Func<T, bool>> expr) => AndImpl(expr);

    private CriteriaChain<T> AndImpl(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        // Enforce that groups (detached chains) must start with Where
        // But allow And() after Where() has been called
        if (_isDetachedGroup && Root is null)
        {
            throw new InvalidOperationException(
                "Groups must start with Where(). Use AndGroup(g => g.Where(...).And(...)) instead of AndGroup(g => g.And(...))");
        }

        var node = new PredicateNode<T>(expr);
        var oldRoot = Root;
        Root = Root is null ? node : new AndNode<T>(Root, node);

        if (_attachedSpec is not null)
        {
            // If this is the first node in the chain, merge it
            // Otherwise, we need to update the spec's tree by removing old root and adding new root
            if (oldRoot is null)
            {
                _attachedSpec.MergeIntoTree(node, combineAsOr: false);
            }
            else
            {
                // Remove old root and add new root
                _attachedSpec.ReplaceInTree(oldRoot, Root);
            }
        }

        return this;
    }

    public ICriteriaChain<T> Or(Expression<Func<T, bool>> expr) => OrImpl(expr);
    IWhereBuilder<T> IWhereBuilder<T>.Or(Expression<Func<T, bool>> expr) => OrImpl(expr);

    private CriteriaChain<T> OrImpl(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        // Enforce that groups (detached chains) must start with Where
        // But allow And/Or/Not as first operation in Where(builder) context (attached but no root yet)
        if (_isDetachedGroup && Root is null)
        {
            throw new InvalidOperationException(
                "Groups must start with Where(). Use Group(g => g.Where(...).Or(...)) instead of Group(g => g.Or(...))");
        }

        var node = new PredicateNode<T>(expr);
        var oldRoot = Root;
        Root = Root is null ? node : new OrNode<T>(Root, node);

        if (_attachedSpec is not null)
        {
            // If this is the first node in the chain, merge it
            // Otherwise, we need to update the spec's tree by removing old root and adding new root
            if (oldRoot is null)
            {
                _attachedSpec.MergeIntoTree(node, combineAsOr: true);
            }
            else
            {
                // Remove old root and add new root
                _attachedSpec.ReplaceInTree(oldRoot, Root);
            }
        }

        return this;
    }


    public ICriteriaChain<T> AndGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group) => AndGroupImpl(group);
    IWhereBuilder<T> IWhereBuilder<T>.AndGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group) => AndGroupImpl(group);

    private CriteriaChain<T> AndGroupImpl(Func<ICriteriaChain<T>, ICriteriaChain<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var detached = StartDetached();
        var built = group(detached) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid group builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added. Groups must start with Where().");

        Root = Root is null ? built.Root : new AndNode<T>(Root, built.Root);

        if (_attachedSpec is not null)
            _attachedSpec.MergeIntoTree(built.Root, combineAsOr: false);

        return this;
    }

    public ICriteriaChain<T> OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group) => OrGroupImpl(group);
    IWhereBuilder<T> IWhereBuilder<T>.OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group) => OrGroupImpl(group);

    private CriteriaChain<T> OrGroupImpl(Func<ICriteriaChain<T>, ICriteriaChain<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var detached = StartDetached();
        var built = group(detached) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid group builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added. Groups must start with Where().");

        Root = Root is null ? built.Root : new OrNode<T>(Root, built.Root);

        if (_attachedSpec is not null)
            _attachedSpec.MergeIntoTree(built.Root, combineAsOr: true);

        return this;
    }
}

