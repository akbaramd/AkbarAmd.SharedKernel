/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork Interfaces
 * Advanced and comprehensive repository interfaces with batch, projection, cancellation, and transactional support.
 * Year: 2025
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;
using CleanArchitecture.Domain.SharedKernel.Interfaces;
using CleanArchitecture.Domain.SharedKernel.Models;

namespace CleanArchitecture.Domain.SharedKernel.Interfaces
{
    public interface IReadOnlyRepository<T, TKey>
        where T : EntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        // Basic queries

        Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        Task<T> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T> FindOneAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        Task<T?> FindOneOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<T?> FindOneOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        // Existence checks

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        // Counting

        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        // Paging support
        Task<PaginatedResult<T>> GetPaginatedAsync(IPaginatedSpecification<T> specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, ISpecification<T> specification, CancellationToken cancellationToken = default);

        // Projection (select specific fields)

        Task<TResult> FindOneAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default);

        Task<TResult?> FindOneOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default);

        Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default);

        // Tracking options (optional, depending on ORM)

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken = default);

        Task<T?> FindOneOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken = default);

        // Transactional support (optional)

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }

    public interface IRepository<T, TKey> : IReadOnlyRepository<T, TKey>
        where T : EntityBase<TKey>
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
