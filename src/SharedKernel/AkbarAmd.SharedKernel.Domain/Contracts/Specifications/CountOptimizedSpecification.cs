using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Wrapper specification optimized for count operations.
/// Simply returns the criteria from the wrapped specification.
/// Since specifications now only contain criteria, this is a simple pass-through.
/// </summary>
public sealed class CountOptimizedSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _spec;

    /// <summary>
    /// Initializes a new instance of the CountOptimizedSpecification class.
    /// </summary>
    /// <param name="spec">The specification to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when spec is null.</exception>
    public CountOptimizedSpecification(ISpecification<T> spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    /// <summary>
    /// Gets the criteria expression from the wrapped specification.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria => _spec.Criteria;

    /// <summary>
    /// Determines whether a candidate object satisfies the wrapped specification.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate satisfies the specification; otherwise, false.</returns>
    public bool IsSatisfiedBy(T candidate) => _spec.IsSatisfiedBy(candidate);

    /// <summary>
    /// Converts the wrapped specification to an expression tree for use in queries.
    /// </summary>
    /// <returns>The criteria expression, or null if no criteria are defined.</returns>
    public Expression<Func<T, bool>>? ToExpression() => _spec.ToExpression();
}
