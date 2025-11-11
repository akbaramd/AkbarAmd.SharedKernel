using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Wrapper specification optimized for count operations.
/// Removes paging and includes to improve performance while preserving criteria.
/// Sorting is removed as it's not needed for count operations.
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
    /// Gets an empty list of includes (removed for count operations).
    /// </summary>
    public IReadOnlyList<Expression<Func<T, object>>> Includes { get; } = Array.Empty<Expression<Func<T, object>>>();

    /// <summary>
    /// Gets an empty list of include strings (removed for count operations).
    /// </summary>
    public IReadOnlyList<string> IncludeStrings { get; } = Array.Empty<string>();

    /// <summary>
    /// Always returns null (sorting not needed for count).
    /// </summary>
    public Expression<Func<T, object>>? OrderBy => null;

    /// <summary>
    /// Always returns null (sorting not needed for count).
    /// </summary>
    public Expression<Func<T, object>>? OrderByDescending => null;

    /// <summary>
    /// Always returns 0 (paging disabled).
    /// </summary>
    public int Take => 0;

    /// <summary>
    /// Always returns 0 (paging disabled).
    /// </summary>
    public int Skip => 0;

    /// <summary>
    /// Always returns false (paging disabled).
    /// </summary>
    public bool IsPagingEnabled => false;
}

