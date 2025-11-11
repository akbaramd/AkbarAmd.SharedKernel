using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Fluent builder for creating ad-hoc specifications without inheriting from BaseSpecification.
/// Provides a clean API for building complete specifications with criteria, includes, sorting, and paging.
/// </summary>
/// <typeparam name="T">The entity type for the specification.</typeparam>
public sealed class FluentSpecificationBuilder<T>
{
    private sealed class AdHocSpecification : BaseSpecification<T>
    {
        // Public wrapper methods to expose protected BaseSpecification methods
        // We ignore the return value since we're building through FluentSpecificationBuilder
        public void AddWhere(Expression<Func<T, bool>> start)
        {
            _ = base.Where(start);
        }

        public void AddWhere(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
        {
            _ = base.Where(builder);
        }
    }

    private readonly AdHocSpecification _spec = new();

    /// <summary>
    /// Adds a criteria expression using the fluent Where API.
    /// </summary>
    public FluentSpecificationBuilder<T> Where(Expression<Func<T, bool>> expr)
    {
        if (expr is null)
            throw new ArgumentNullException(nameof(expr));
        
        _spec.AddWhere(expr);
        return this;
    }

    /// <summary>
    /// Adds complex criteria using a builder function.
    /// </summary>
    public FluentSpecificationBuilder<T> Where(Func<ICriteriaChain<T>, ICriteriaChain<T>> builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));
        
        _spec.AddWhere(builder);
        return this;
    }

    /// <summary>
    /// Adds an include expression.
    /// </summary>
    public FluentSpecificationBuilder<T> Include(Expression<Func<T, object>> include)
    {
        _spec.Include(include);
        return this;
    }

    /// <summary>
    /// Adds an include by string path.
    /// </summary>
    public FluentSpecificationBuilder<T> Include(string path)
    {
        _spec.Include(path);
        return this;
    }

    /// <summary>
    /// Adds multiple include expressions.
    /// </summary>
    public FluentSpecificationBuilder<T> Include(params Expression<Func<T, object>>[] includes)
    {
        _spec.Include(includes);
        return this;
    }

    /// <summary>
    /// Adds multiple include paths.
    /// </summary>
    public FluentSpecificationBuilder<T> IncludePaths(params string[] paths)
    {
        _spec.IncludePaths(paths);
        return this;
    }

    /// <summary>
    /// Sets the primary sort key in ascending order.
    /// </summary>
    public FluentSpecificationBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> key)
    {
        _spec.OrderByKey(key);
        return this;
    }

    /// <summary>
    /// Sets the primary sort key in descending order.
    /// </summary>
    public FluentSpecificationBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        _spec.OrderByKeyDescending(key);
        return this;
    }

    /// <summary>
    /// Adds a secondary sort key in ascending order.
    /// </summary>
    public FluentSpecificationBuilder<T> ThenBy<TKey>(Expression<Func<T, TKey>> key)
    {
        _spec.ThenBy(key);
        return this;
    }

    /// <summary>
    /// Adds a secondary sort key in descending order.
    /// </summary>
    public FluentSpecificationBuilder<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> key)
    {
        _spec.ThenByDescending(key);
        return this;
    }

    /// <summary>
    /// Sets null ordering policy to NullsFirst for the last sort descriptor.
    /// </summary>
    public FluentSpecificationBuilder<T> NullsFirst()
    {
        _spec.NullsFirst();
        return this;
    }

    /// <summary>
    /// Sets null ordering policy to NullsLast for the last sort descriptor.
    /// </summary>
    public FluentSpecificationBuilder<T> NullsLast()
    {
        _spec.NullsLast();
        return this;
    }

    /// <summary>
    /// Sets pagination using page number and page size.
    /// </summary>
    public FluentSpecificationBuilder<T> Page(int page, int size)
    {
        _spec.Page(page, size);
        return this;
    }

    /// <summary>
    /// Sets the skip value for pagination.
    /// </summary>
    public FluentSpecificationBuilder<T> Skip(int skip)
    {
        _spec.SkipBy(skip);
        return this;
    }

    /// <summary>
    /// Sets the take value for pagination.
    /// </summary>
    public FluentSpecificationBuilder<T> Take(int take)
    {
        _spec.TakeBy(take);
        return this;
    }

    /// <summary>
    /// Builds and returns the final specification.
    /// </summary>
    public ISpecification<T> Build() => _spec;
}

