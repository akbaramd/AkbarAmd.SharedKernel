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
using AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Repositories
{
    public abstract class ReadOnlyEfRepository<TDbContext, T, TKey> : IReadOnlyRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        protected readonly TDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        protected readonly bool _enableTracking;
        protected readonly ISpecificationEvaluator<T> _evaluator;

        protected ReadOnlyEfRepository(TDbContext dbContext, ISpecificationEvaluator<T>? evaluator = null, bool enableTracking = false)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
            _enableTracking = enableTracking;
            // Use provided evaluator or default to EF Core implementation
            _evaluator = evaluator ?? new EfCoreSpecificationEvaluator<T>();
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

        /// <summary>
        /// Applies specification to query. Returns queryable with criteria applied.
        /// </summary>
        protected IQueryable<T> ApplySpecification(ISpecification<T>? specification)
        {
            var baseQuery = PrepareQuery(_dbSet);
            if (specification is null)
                return baseQuery;

            return _evaluator.Evaluate(baseQuery, specification, BuildEvaluatorOptions());
        }

        // ---------- Infrastructure-level query building (sorting, pagination)

        /// <summary>
        /// Applies sorting to a query. Override this method to customize sorting behavior.
        /// </summary>
        protected virtual IQueryable<T> ApplySorting(
            IQueryable<T> query,
            Expression<Func<T, object>>? orderBy,
            SortDirection direction = SortDirection.Ascending)
        {
            if (orderBy == null)
            {
                // Apply stable sorting by ID if no explicit sort is provided
                return query.OrderBy(x => x.Id);
            }

            // Convert decimal to double for SQLite compatibility
            var convertedOrderBy = EfCoreSpecificationEvaluator<T>.ConvertDecimalToDoubleIfNeeded(orderBy);

            return direction == SortDirection.Descending
                ? query.OrderByDescending(convertedOrderBy)
                : query.OrderBy(convertedOrderBy);
        }


        /// <summary>
        /// Applies pagination to a query.
        /// </summary>
        protected virtual IQueryable<T> ApplyPagination(IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
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
            => await ApplySpecification(specification).CountAsync(cancellationToken);

        // ========== Paginated Results

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
        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy = null,
            SortDirection direction = SortDirection.Ascending,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            // Get total count
            var totalCount = predicate is null
                ? await CountAsync(cancellationToken)
                : await CountAsync(predicate, cancellationToken);

            // Get paged items
            var query = PrepareQuery(_dbSet);
            if (predicate != null)
                query = query.Where(predicate);
            
            query = ApplySorting(query, orderBy, direction);
            query = ApplyPagination(query, pageNumber, pageSize);
            
            var items = await query.ToListAsync(cancellationToken);
            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
        }

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
        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(
            ISpecification<T> specification,
            int pageNumber,
            int pageSize,
            Expression<Func<T, object>>? orderBy = null,
            SortDirection direction = SortDirection.Ascending,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            // Get total count
            var totalCount = specification is null
                ? await CountAsync(cancellationToken)
                : await CountAsync(specification, cancellationToken);

            // Get paged items
            var query = ApplySpecification(specification);
            query = ApplySorting(query, orderBy, direction);
            query = ApplyPagination(query, pageNumber, pageSize);
            
            var items = await query.ToListAsync(cancellationToken);
            return new PaginatedResult<T>(items, totalCount, pageNumber, pageSize);
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




    /// <summary>
    /// Full EF Core repository supporting write operations, inherits read-only repo.
    /// </summary>
    public abstract class EfRepository<TDbContext, T, TKey> : ReadOnlyEfRepository<TDbContext, T, TKey>, IRepository<T, TKey>
        where T : Entity<TKey>
        where TKey : IEquatable<TKey>
        where TDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the EfRepository class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="evaluator">Optional specification evaluator. If not provided, uses EF Core implementation.</param>
        protected EfRepository(TDbContext dbContext, ISpecificationEvaluator<T>? evaluator = null) 
            : base(dbContext, evaluator, enableTracking: true)
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
