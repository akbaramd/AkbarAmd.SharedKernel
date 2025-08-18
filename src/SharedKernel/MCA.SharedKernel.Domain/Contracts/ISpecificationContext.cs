/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Specification Context Interface
 * Interface for specification context operations, keeping domain independent of infrastructure.
 * Year: 2025
 */

using System.Linq.Expressions;

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Interface for specification context operations, providing query manipulation capabilities.
    /// This interface keeps the domain layer independent of specific infrastructure implementations.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public interface ISpecificationContext<T>
    {
        /// <summary>
        /// Gets the current query being manipulated.
        /// </summary>
        IQueryable<T> Query { get; }

        /// <summary>
        /// Adds a filter condition to the query.
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        void AddFilter(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds ordering to the query.
        /// </summary>
        /// <typeparam name="TKey">The type of the key to order by</typeparam>
        /// <param name="keySelector">The key selector expression</param>
        /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
        void AddOrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true);

        /// <summary>
        /// Adds pagination to the query.
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        void AddPagination(int skip, int take);

        /// <summary>
        /// Adds an include to the query for navigation properties.
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        void AddInclude(Expression<Func<T, object>> includeExpression);

        /// <summary>
        /// Adds a string include to the query.
        /// </summary>
        /// <param name="includeString">The include string</param>
        void AddInclude(string includeString);

        /// <summary>
        /// Adds then ordering to the query (for subsequent ordering after initial order).
        /// </summary>
        /// <typeparam name="TKey">The type of the key to order by</typeparam>
        /// <param name="keySelector">The key selector expression</param>
        /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
        void AddThenOrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true);

        /// <summary>
        /// Sets the query to no tracking mode.
        /// </summary>
        void SetNoTracking();

        /// <summary>
        /// Adds a filter condition to the query with no tracking.
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        void AddFilterNoTracking(Expression<Func<T, bool>> predicate);
    }
} 