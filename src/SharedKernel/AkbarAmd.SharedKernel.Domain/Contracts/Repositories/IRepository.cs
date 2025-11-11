/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork Reporitories
 * Advanced and comprehensive repository interfaces with batch, projection, cancellation, and transactional support.
 * Year: 2025
 */

using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Repositories
{
    public interface IRepository<T, in TKey> : IReadOnlyRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        // Single entity modifications

        Task AddAsync(T entity, bool save = false, CancellationToken cancellationToken = default);

        Task UpdateAsync(T entity, bool save = false, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, bool save = false, CancellationToken cancellationToken = default);

        // Batch operations

        Task AddRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);

        Task UpdateRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default);

        // Bulk operations using ExecuteDeleteAsync and ExecuteUpdateAsync

        /// <summary>
        /// Bulk delete entities that match the specified condition using ExecuteDeleteAsync
        /// </summary>
        /// <param name="predicate">Condition to match entities for deletion</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities deleted</returns>
        Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);


        // Save changes explicitly
        Task SaveAsync(CancellationToken cancellationToken = default);

   
    }
}
