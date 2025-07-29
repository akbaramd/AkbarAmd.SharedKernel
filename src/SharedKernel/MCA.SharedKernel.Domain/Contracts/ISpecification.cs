/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Specifications
 * Specification interfaces supporting filtering, paging, and sorting with separate fields and directions.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Base specification interface providing query composition for entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Applies the specification logic to the provided <see cref="IQueryable{T}"/>.
        /// This includes filtering, sorting, paging, and other query customizations.
        /// </summary>
        /// <param name="queryable">The input queryable to which the specification is applied.</param>
        /// <returns>An <see cref="IQueryable{T}"/> modified according to the specification.</returns>
        IQueryable<T> Query(IQueryable<T> queryable);
    }

    /// <summary>
    /// Specification interface extending <see cref="ISpecification{T}"/> with pagination support.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public interface IPaginatedSpecification<T> : ISpecification<T>
    {
        /// <summary>
        /// Gets the number of records to skip in the result set.
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Gets the maximum number of records to take from the result set.
        /// </summary>
        int Take { get; }
    }

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

    /// <summary>
    /// Specification interface extending <see cref="ISortedSpecification{T}"/> with filtering capabilities.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public interface IFilterableSpecification<T> : ISortedSpecification<T>
    {
        /// <summary>
        /// Gets the search string used for filtering the result set.
        /// </summary>
        string Search { get; }
    }
}
