using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Models;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Repositories
{
    /// <summary>
    /// Read-only repository abstraction for querying entities with specifications, projections, sorting, and pagination.
    /// </summary>
    public interface IReadOnlyRepository<T, in TKey>
        where T : IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        // -------------------------
        // 🔹 Basic queries
        // -------------------------

        Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> FindOneAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        // -------------------------
        // 🔹 Counting
        // -------------------------
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        // -------------------------
        // 🔹 Simple Paging (no specification)
        // -------------------------

        /// <summary>
        /// Returns entities for a specific page using deterministic Id ordering.
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns entities for a specific page using optional specification for filtering and includes.
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            ISpecification<T>? specification,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns entities for a page with optional sorting.
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy,
            SortDirection direction = SortDirection.Ascending,
            ISpecification<T>? specification = null,
            CancellationToken cancellationToken = default);

        // -------------------------
        // 🔹 Paginated Results (with total count)
        // -------------------------

        /// <summary>
        /// Returns a paginated result using basic pagination (no sort).
        /// </summary>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a paginated result using optional specification.
        /// </summary>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            ISpecification<T>? specification,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a paginated result with optional sorting and specification.
        /// </summary>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy,
            SortDirection direction = SortDirection.Ascending,
            ISpecification<T>? specification = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a paginated result using an IPaginatedSpecification that already defines paging behavior.
        /// </summary>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            IPaginatedSpecification<T> specification,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a paginated result using an IPaginatedSortableSpecification that defines both paging and sorting.
        /// </summary>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            IPaginatedSortableSpecification<T> specification,
            CancellationToken cancellationToken = default);

        // -------------------------
        // 🔹 Projection queries
        // -------------------------
        Task<TResult?> FindOneAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> FindAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> GetAllAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        // -------------------------
        // 🔹 Tracking options
        // -------------------------
        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            bool asNoTracking,
            CancellationToken cancellationToken = default);
    }
}
