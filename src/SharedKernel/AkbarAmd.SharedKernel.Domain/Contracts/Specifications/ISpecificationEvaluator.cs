namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Contract for evaluating specifications against IQueryable.
/// Domain layer defines the contract, Infrastructure layer provides the implementation.
/// This allows Domain to remain database-agnostic while Infrastructure handles
/// database-specific evaluation logic (EF Core, MongoDB, etc.).
/// The specification itself IS the schema - its properties define what to apply.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface ISpecificationEvaluator<T>
{
    /// <summary>
    /// Applies a specification to an IQueryable and returns the transformed query.
    /// </summary>
    /// <param name="query">The source IQueryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="options">Optional evaluation options (tracking, split queries, etc.).</param>
    /// <returns>The transformed IQueryable with all specification rules applied.</returns>
    IQueryable<T> Evaluate(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null);

    /// <summary>
    /// Applies a specification optimized for counting (excludes includes and paging).
    /// </summary>
    /// <param name="query">The source IQueryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="options">Optional evaluation options.</param>
    /// <returns>The transformed IQueryable optimized for counting.</returns>
    IQueryable<T> EvaluateForCount(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null);

    /// <summary>
    /// Checks if any items match the specification criteria.
    /// </summary>
    /// <param name="query">The source IQueryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="options">Optional evaluation options.</param>
    /// <returns>True if any items match, false otherwise.</returns>
    bool Any(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null);

    /// <summary>
    /// Counts items matching the specification criteria.
    /// </summary>
    /// <param name="query">The source IQueryable.</param>
    /// <param name="specification">The specification to apply.</param>
    /// <param name="options">Optional evaluation options.</param>
    /// <returns>The count of matching items.</returns>
    int Count(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null);
}

