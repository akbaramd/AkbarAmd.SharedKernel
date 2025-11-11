using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Specifications.Internal;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Implementation of ICriteriaChain that manages criteria building with fluent API.
/// Supports both attached mode (connected to BaseSpecification) and detached mode (for groups).
/// </summary>
internal sealed class CriteriaChain<T> : ICriteriaChain<T>
{
    private readonly BaseSpecification<T>? _attachedSpec; // null => detached group chain
    internal ICriteriaNode<T>? Root { get; private set; }

    private CriteriaChain(BaseSpecification<T>? attached)
    {
        _attachedSpec = attached;
    }

    /// <summary>
    /// Starts a chain attached to a specification with an initial expression.
    /// </summary>
    public static CriteriaChain<T> StartAttached(BaseSpecification<T> spec, Expression<Func<T, bool>> start, bool combineAsOr)
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
    public static CriteriaChain<T> AttachOnExisting(BaseSpecification<T> spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));

        var chain = new CriteriaChain<T>(spec) { Root = spec.CriteriaTree };
        return chain;
    }

    /// <summary>
    /// Starts a detached chain for use in Group/OrGroup.
    /// </summary>
    public static CriteriaChain<T> StartDetached() => new CriteriaChain<T>(attached: null);

    public ICriteriaChain<T> And(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

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

    public ICriteriaChain<T> Or(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

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

    public ICriteriaChain<T> Not(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));

        var notNode = new NotNode<T>(new PredicateNode<T>(expr));
        Root = Root is null ? notNode : new AndNode<T>(Root, notNode); // Default: Not combines with AND

        if (_attachedSpec is not null)
            _attachedSpec.MergeIntoTree(notNode, combineAsOr: false);

        return this;
    }

    public ICriteriaChain<T> Group(Func<ICriteriaChain<T>, ICriteriaChain<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var detached = StartDetached();
        var built = group(detached) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid group builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added.");

        Root = Root is null ? built.Root : new AndNode<T>(Root, built.Root);

        if (_attachedSpec is not null)
            _attachedSpec.MergeIntoTree(built.Root, combineAsOr: false);

        return this;
    }

    public ICriteriaChain<T> OrGroup(Func<ICriteriaChain<T>, ICriteriaChain<T>> group)
    {
        if (group is null)
            throw new ArgumentNullException(nameof(group));

        var detached = StartDetached();
        var built = group(detached) as CriteriaChain<T>
                    ?? throw new InvalidOperationException("Invalid group builder.");

        if (built.Root is null)
            throw new InvalidOperationException("Group cannot be empty. At least one condition must be added.");

        Root = Root is null ? built.Root : new OrNode<T>(Root, built.Root);

        if (_attachedSpec is not null)
            _attachedSpec.MergeIntoTree(built.Root, combineAsOr: true);

        return this;
    }
}

