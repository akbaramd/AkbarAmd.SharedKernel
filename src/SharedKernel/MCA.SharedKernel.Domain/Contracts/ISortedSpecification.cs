namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Specification interface extending <see cref="IPaginatedSpecification{T}"/> with sorting metadata.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public interface ISortedSpecification<T> : IPaginatedSpecification<T>
{
    /// <summary>
    /// Gets the index or key representing which field to sort by.
    /// Interpretation of this index is implementation-specific.
    /// </summary>
    int SortedBy { get; }

    /// <summary>
    /// Gets the sorting direction, typically 0 for ascending and 1 for descending.
    /// </summary>
    int SortedDirection { get; }
}