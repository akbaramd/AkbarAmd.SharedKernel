namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Options for evaluating specifications.
/// Used by evaluator services to control query behavior.
/// </summary>
public sealed class SpecificationEvaluationOptions
{
    /// <summary>
    /// Whether to use AsNoTracking for the query.
    /// </summary>
    public bool AsNoTracking { get; init; } = true;

    /// <summary>
    /// Whether to use split queries (EF Core specific).
    /// </summary>
    public bool UseSplitQuery { get; init; } = false;

    /// <summary>
    /// Whether to ignore auto-includes.
    /// </summary>
    public bool IgnoreAutoIncludes { get; init; } = false;

    /// <summary>
    /// Whether to ignore query filters.
    /// </summary>
    public bool IgnoreQueryFilters { get; init; } = false;

    /// <summary>
    /// Whether to apply stable sorting by ID when no explicit sort is provided.
    /// </summary>
    public bool StableSortByIdWhenMissing { get; init; } = true;

    /// <summary>
    /// Whether to use identity resolution when no tracking (EF Core specific).
    /// </summary>
    public bool UseIdentityResolutionWhenNoTracking { get; init; } = false;

    /// <summary>
    /// Optional query tag for debugging/monitoring.
    /// </summary>
    public string? QueryTag { get; init; } = null;
}

