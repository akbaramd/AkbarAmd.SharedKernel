/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Infrastructure - EF Core ReadOnly Repository Base and Full Repository Implementation
 * ReadOnlyEfRepository provides read operations, EfRepository adds write operations.
 * Year: 2025
 */

using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain;
using AkbarAmd.SharedKernel.Domain.Contracts.Repositories;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using AkbarAmd.SharedKernel.Domain.Models;
using AkbarAmd.SharedKernel.Infrastructure.Specifications;
using Microsoft.EntityFrameworkCore;
// removed: using AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications; (no longer needed with simplified specifications)

namespace AkbarAmd.SharedKernel.Infrastructure.Repositories
{
    public abstract class ReadOnlyEfRepository<TDbContext, T, TKey> : IReadOnlyRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        protected readonly TDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        protected readonly bool _enableTracking;

        protected ReadOnlyEfRepository(TDbContext dbContext, bool enableTracking = false)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
            _enableTracking = enableTracking;
        }

        // ---------- Options for evaluator (override per repo if needed)
        protected virtual SpecificationEvaluationOptions BuildEvaluatorOptions() => new()
        {
            AsNoTracking = !_enableTracking,
            UseSplitQuery = false,
            IgnoreAutoIncludes = false,
            IgnoreQueryFilters = false,
            StableSortByIdWhenMissing = true,
            UseIdentityResolutionWhenNoTracking = false,
            QueryTag = null
        };

        // ---------- Plain prepare for non-spec queries
        protected virtual IQueryable<T> PrepareQuery(IQueryable<T> query)
            => _enableTracking ? query : query.AsNoTracking();

        protected IQueryable<T> ApplySpecification(ISpecification<T>? specification)
        {
            var baseQuery = _dbSet.AsQueryable();
            if (specification is null)
                return BuildEvaluatorOptions().AsNoTracking ? baseQuery.AsNoTracking() : baseQuery;

            return EfCoreSpecificationEvaluator<T>.GetQuery(baseQuery, specification, BuildEvaluatorOptions());
        }

        private IQueryable<T> ApplySpecificationForCount(ISpecification<T> specification)
        {
            var baseQuery = _dbSet.AsQueryable();
            var countOptimized = new CountOptimizedSpecification<T>(specification);
            return EfCoreSpecificationEvaluator<T>.GetQuery(baseQuery, countOptimized, BuildEvaluatorOptions());
        }

        // ========== Core Queries
        public virtual async Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            if (EqualityComparer<TKey>.Default.Equals(id, default))
                throw new ArgumentException("Id cannot be null or default value.", nameof(id));

            var query = PrepareQuery(_dbSet);
            return await query.FirstAsync(x => x.Id!.Equals(id), cancellationToken);
        }

        public virtual async Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).FirstOrDefaultAsync(predicate, cancellationToken);

        public virtual async Task<T?> FindOneAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
            => await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).Where(predicate).ToListAsync(cancellationToken);

        public virtual async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
            => await ApplySpecification(specification).ToListAsync(cancellationToken);

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).AnyAsync(predicate, cancellationToken);

        public virtual async Task<bool> ExistsAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
            => await ApplySpecification(specification).AnyAsync(cancellationToken);

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).CountAsync(cancellationToken);

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).CountAsync(predicate, cancellationToken);

        public virtual async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
            => await ApplySpecificationForCount(specification).CountAsync(cancellationToken);

        // ========== Simple Paging without spec
        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = PrepareQuery(_dbSet);

            // Stable ordering by key for consistent paging
            query = query.OrderBy(x => x.Id);

            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, ISpecification<T>? specification, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            // Remove paging from specification to avoid double paging
            // Evaluator will apply stable sorting if no explicit sort is provided
            var countOptimized = new CountOptimizedSpecification<T>(specification);
            var query = EfCoreSpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), countOptimized, BuildEvaluatorOptions());

            return await query.Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy,
            SortDirection direction = SortDirection.Ascending,
            ISpecification<T>? specification = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var query = ApplySpecification(specification);

            if (orderBy != null)
            {
                // Convert decimal to double for SQLite compatibility
                var convertedOrderBy = EfCoreSpecificationEvaluator<T>.ConvertDecimalToDoubleIfNeeded(orderBy);
                query = direction == SortDirection.Descending ? query.OrderByDescending(convertedOrderBy) : query.OrderBy(convertedOrderBy);
            }

            // اگر orderBy null بود، Evaluator با StableSortByIdWhenMissing پوشش می‌دهد.

            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync(cancellationToken);
        }

        // ========== Paginated Results
        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
            => await GetPaginatedAsync(pageNumber, pageSize, specification: null, cancellationToken);

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(int pageNumber, int pageSize, ISpecification<T>? specification, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var totalCount = specification is null
                ? await CountAsync(cancellationToken)
                : await CountAsync(specification, cancellationToken);

            var items = specification is null
                ? await GetPagedAsync(pageNumber, pageSize, cancellationToken)
                : await GetPagedAsync(pageNumber, pageSize, specification, cancellationToken);

            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy,
            SortDirection direction = SortDirection.Ascending,
            ISpecification<T>? specification = null,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var totalCount = specification is null
                ? await CountAsync(cancellationToken)
                : await CountAsync(specification, cancellationToken);

            var items = await GetPagedAsync(pageNumber, pageSize, orderBy, direction, specification, cancellationToken);
            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(IPaginatedSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            // پیجینگ داخل Spec اعمال شده
            var pageQuery = ApplySpecification(specification);

            // شمارش بدون Paging و Include
            var totalCount = await ApplySpecificationForCount(specification).CountAsync(cancellationToken);

            var items = await pageQuery.ToListAsync(cancellationToken);
            return new PaginatedResult<T>(items, totalCount, specification.PageNumber, specification.PageSize);
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(IPaginatedSortableSpecification<T> specification, CancellationToken cancellationToken = default)
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            var pageQuery = ApplySpecification(specification);
            var totalCount = await ApplySpecificationForCount(specification).CountAsync(cancellationToken);

            var items = await pageQuery.ToListAsync(cancellationToken);
            return new PaginatedResult<T>(items, totalCount, specification.PageNumber, specification.PageSize);
        }

        // ========== Projection
        public virtual async Task<TResult?> FindOneAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);

        public virtual async Task<IEnumerable<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).Where(predicate).Select(selector).ToListAsync(cancellationToken);

        public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken cancellationToken = default)
            => await PrepareQuery(_dbSet).Select(selector).ToListAsync(cancellationToken);

        // ========== Tracking options
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet.AsQueryable();
            return await query.Where(predicate).ToListAsync(cancellationToken);
        }
    }

    // For cases where you need to count all entities (null spec)
    internal sealed class EmptySpecification<T> : ISpecification<T>
    {
        public Expression<Func<T, bool>> Criteria => x => true;
        public IReadOnlyList<Expression<Func<T, object>>> Includes { get; } = Array.Empty<Expression<Func<T, object>>>();
        public IReadOnlyList<string> IncludeStrings { get; } = Array.Empty<string>();
        public Expression<Func<T, object>>? OrderBy => null;
        public Expression<Func<T, object>>? OrderByDescending => null;
        public int Take => 0;
        public int Skip => 0;
        public bool IsPagingEnabled => false;
    }



    /// <summary>
    /// Full EF Core repository supporting write operations, inherits read-only repo.
    /// </summary>
    public abstract class EfRepository<TDbContext, T, TKey> : ReadOnlyEfRepository<TDbContext, T, TKey>, IRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        protected EfRepository(TDbContext dbContext) : base(dbContext, enableTracking: true)
        {
        }

        #region Write Operations

        /// <summary>
        /// Adds an entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddAsync(T entity, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity, cancellationToken);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Updates an entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateAsync(T entity, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task DeleteAsync(T entity, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Remove(entity);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Adds multiple entities to the repository
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities, cancellationToken);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Updates multiple entities in the repository
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.UpdateRange(entities);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Deletes multiple entities from the repository
        /// </summary>
        /// <param name="entities">The entities to delete</param>
        /// <param name="save">If true, automatically calls SaveChangesAsync</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, bool save = false, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbSet.RemoveRange(entities);

            if (save)
            {
                await SaveAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Bulk delete entities that match the specified condition using ExecuteDeleteAsync
        /// This is more efficient than loading entities into memory and then deleting them
        /// </summary>
        /// <param name="predicate">Condition to match entities for deletion</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of entities deleted</returns>
        public virtual async Task<int> BulkDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var query = PrepareQuery(_dbSet);
            return await query.Where(predicate).ExecuteDeleteAsync(cancellationToken);
        }


        /// <summary>
        /// Saves all changes made in this context to the database
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public virtual async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion
    }
}
