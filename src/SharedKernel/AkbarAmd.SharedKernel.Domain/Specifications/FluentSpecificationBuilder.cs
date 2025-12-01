using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Fluent builder for creating ad-hoc specifications without inheriting from BaseSpecification.
/// Provides a clean API for building domain specifications with criteria only.
/// Pure domain specification - no infrastructure concerns (includes, sorting, pagination).
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
    /// Builds and returns the final specification.
    /// </summary>
    public ISpecification<T> Build() => _spec;
}
