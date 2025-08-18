/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork Reporitories
 * Advanced and comprehensive repository interfaces with batch, projection, cancellation, and transactional support.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Contracts
{
    public interface IRepository<T, in TKey> : IReadOnlyRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
    {
        // Single entity modifications

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        // Batch operations

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Save changes

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
