/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Infrastructure - EF Core ReadOnly Repository Base and Full Repository Implementation
 * ReadOnlyEfRepository provides read operations, EfRepository adds write operations.
 * Year: 2025
 */

using System.Linq.Expressions;
using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;
using MCA.SharedKernel.Domain.Models;
using MCA.SharedKernel.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MCA.SharedKernel.Infrastructure.Repositories
{
    /// <summary>
    /// Base abstract read-only repository implementation using EF Core.
    /// </summary>
    public abstract class ReadOnlyEfRepository<TDbContext, T, TKey> : IReadOnlyRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        protected readonly TDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        protected ReadOnlyEfRepository(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        /// <summary>
        /// Virtual method to prepare query with includes and other modifications.
        /// Override this method in derived repositories to add includes, filters, etc.
        /// </summary>
        /// <param name="query">The base query</param>
        /// <returns>The prepared query with modifications</returns>
        protected virtual IQueryable<T> PrepareQuery(IQueryable<T> query)
        {
            return query;
        }

        protected IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            if (specification == null)
                return PrepareQuery(_dbSet.AsQueryable());

            var context = new EfSpecificationContext<T>(PrepareQuery(_dbSet.AsQueryable()));
            specification.Handle(context);
            return context.Query;
        }

        public virtual async Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public virtual async Task<T> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T> FindOneAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<T?> FindOneOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual async Task<T?> FindOneOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.AnyAsync(predicate, cancellationToken);
        }

        public virtual async Task<bool> ExistsAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.AnyAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.CountAsync(predicate, cancellationToken);
        }

        public virtual async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            var query = ApplySpecification(specification);
            return await query.CountAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = ApplySpecification(specification)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<TResult> FindOneAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<TResult?> FindOneOrDefaultAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.Where(predicate).Select(selector).ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());
            return await query.Select(selector).ToListAsync(cancellationToken);
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(IPaginatedSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            var query = ApplySpecification(specification);
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip(specification.Skip).Take(specification.Take).ToListAsync(cancellationToken);

            int pageNumber = specification.Skip / specification.Take + 1;

            return new PaginatedResult<T>(items, totalCount, pageNumber, specification.Take);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual async Task<T?> FindOneOrDefaultAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = PrepareQuery(_dbSet.AsQueryable());

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public virtual Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // ReadOnly repo does not support transactions
            throw new NotSupportedException("BeginTransaction is not supported in read-only repository.");
        }

        public virtual Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            // ReadOnly repo does not support transactions
            throw new NotSupportedException("CommitTransaction is not supported in read-only repository.");
        }

        public virtual Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            // ReadOnly repo does not support transactions
            throw new NotSupportedException("RollbackTransaction is not supported in read-only repository.");
        }
    }

    /// <summary>
    /// Full EF Core repository supporting write operations, inherits read-only repo.
    /// </summary>
    public abstract class EfRepository<TDbContext, T, TKey> : ReadOnlyEfRepository<TDbContext, T, TKey>, IRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        private IDbContextTransaction _transaction;

        protected EfRepository(TDbContext dbContext) : base(dbContext)
        {
        }

        #region Write Operations

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Transaction Support

        public override async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public override async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public override async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        #endregion
    }
}
