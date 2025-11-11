namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Defines a specification that supports multi-level sorting with ThenBy/ThenByDescending.
/// Allows chaining multiple sort criteria with null ordering policies.
/// </summary>
/// <typeparam name="T">The entity type being sorted.</typeparam>
public interface IMultiSortSpecification<T>
{
    /// <summary>
    /// Gets the ordered list of sort descriptors.
    /// The first descriptor is the primary sort, subsequent descriptors are secondary sorts.
    /// </summary>
    IReadOnlyList<SortDescriptor<T>> Sorts { get; }
}

