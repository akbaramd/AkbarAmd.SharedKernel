/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Infrastructure - EF Core Specification Context
 * Extended specification context with EF Core specific operations.
 * Year: 2025
 */

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.SharedKernel.Infrastructure.Specifications
{
    /// <summary>
    /// EF Core specific specification context implementing the domain interface.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public class EfSpecificationContext<T> : ISpecificationContext<T> where T : class
    {
        public IQueryable<T> Query { get; private set; }

        public EfSpecificationContext(IQueryable<T> query)
        {
            Query = query;
        }

        /// <summary>
        /// Adds a filter condition to the query.
        /// </summary>
        public void AddFilter(Expression<Func<T, bool>> predicate)
        {
            Query = Query.Where(predicate);
        }

        /// <summary>
        /// Adds ordering to the query.
        /// </summary>
        public void AddOrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true)
        {
            if (ascending)
                Query = Query.OrderBy(keySelector);
            else
                Query = Query.OrderByDescending(keySelector);
        }

        /// <summary>
        /// Adds pagination to the query.
        /// </summary>
        public void AddPagination(int skip, int take)
        {
            Query = Query.Skip(skip).Take(take);
        }

        /// <summary>
        /// Adds an include to the query for navigation properties.
        /// </summary>
        public void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Query = Query.Include(includeExpression);
        }

        /// <summary>
        /// Adds a string include to the query.
        /// </summary>
        public void AddInclude(string includeString)
        {
            Query = Query.Include(includeString);
        }

        /// <summary>
        /// Adds then ordering to the query.
        /// </summary>
        public void AddThenOrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true)
        {
            if (Query is IOrderedQueryable<T> orderedQuery)
            {
                if (ascending)
                    Query = orderedQuery.ThenBy(keySelector);
                else
                    Query = orderedQuery.ThenByDescending(keySelector);
            }
        }

        /// <summary>
        /// Sets the query to no tracking mode.
        /// </summary>
        public void SetNoTracking()
        {
            Query = Query.AsNoTracking();
        }

        /// <summary>
        /// Adds a filter condition to the query with no tracking.
        /// </summary>
        public void AddFilterNoTracking(Expression<Func<T, bool>> predicate)
        {
            Query = Query.AsNoTracking().Where(predicate);
        }
    }
} 