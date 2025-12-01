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
        // 🔹 Paginated Results (with total count)
        // -------------------------

        /// <summary>
        /// Returns paginated results using an expression predicate for filtering.
        /// </summary>
        /// <param name="predicate">Optional filter expression. If null, returns all entities.</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="orderBy">Optional sort expression. If null, sorts by ID.</param>
        /// <param name="direction">Sort direction (default: Ascending).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated result with items and total count.</returns>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy = null,
            SortDirection direction = SortDirection.Ascending,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns paginated results using a specification for filtering.
        /// </summary>
        /// <param name="specification">Optional specification for filtering. If null, returns all entities.</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="orderBy">Optional sort expression. If null, sorts by ID.</param>
        /// <param name="direction">Sort direction (default: Ascending).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated result with items and total count.</returns>
        Task<PaginatedResult<T>> GetPaginatedAsync(
            ISpecification<T> specification,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy = null,
            SortDirection direction = SortDirection.Ascending,
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
