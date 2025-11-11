using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Represents a single sort descriptor in a multi-level sorting chain.
/// Contains the key selector, sort direction, and null ordering policy.
/// </summary>
/// <typeparam name="T">The entity type being sorted.</typeparam>
public sealed record SortDescriptor<T>
{
    /// <summary>
    /// The lambda expression that selects the property to sort by.
    /// Example: (T x) => x.Name
    /// </summary>
    public LambdaExpression KeySelector { get; init; }

    /// <summary>
    /// The direction of sorting (Ascending or Descending).
    /// </summary>
    public SortDirection Direction { get; init; }

    /// <summary>
    /// The null ordering policy for this sort level.
    /// Only meaningful for nullable or reference types.
    /// </summary>
    public NullSort Nulls { get; init; }

    /// <summary>
    /// Initializes a new instance of SortDescriptor.
    /// </summary>
    /// <param name="keySelector">The property selector expression.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="nulls">The null ordering policy.</param>
    public SortDescriptor(
        LambdaExpression keySelector,
        SortDirection direction = SortDirection.Ascending,
        NullSort nulls = NullSort.Unspecified)
    {
        KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        Direction = direction;
        Nulls = nulls;
    }
}

