/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Infrastructure - Base Specification Implementation
 * Comprehensive base specification with context-based approach for includes, criteria, ordering, and pagination.
 * Year: 2025
 */

using System.Linq.Expressions;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.SharedKernel.Infrastructure.Specifications
{
    /// <summary>
    /// Base specification implementation showing how to use the context-based approach.
    /// This provides a comprehensive template for creating specific specifications.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        protected List<Expression<Func<T, bool>>> Filters { get; } = new();
        protected List<Expression<Func<T, object>>> Includes { get; } = new();
        protected List<string> IncludeStrings { get; } = new();
        protected List<OrderByExpression<T>> OrderByExpressions { get; } = new();
        protected int Take { get; private set; }
        protected int Skip { get; private set; }
        protected bool IsPagingEnabled { get; private set; }
        protected bool IsNoTrackingEnabled { get; private set; }

        protected record OrderByExpression<TEntity>(Expression<Func<TEntity, object>> Expression, bool Ascending);

        /// <summary>
        /// Adds a filter condition to the specification.
        /// </summary>
        /// <param name="filterExpression">The filter expression</param>
        protected virtual void AddFilter(Expression<Func<T, bool>> filterExpression)
        {
            Filters.Add(filterExpression);
        }

        /// <summary>
        /// Adds an include for navigation properties to the specification.
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Adds a string include to the specification.
        /// </summary>
        /// <param name="includeString">The include string</param>
        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        /// <summary>
        /// Adds ordering to the specification.
        /// </summary>
        /// <param name="orderByExpression">The order by expression</param>
        /// <param name="ascending">Whether to order ascending (true) or descending (false)</param>
        protected virtual void AddOrderBy(Expression<Func<T, object>> orderByExpression, bool ascending = true)
        {
            OrderByExpressions.Add(new OrderByExpression<T>(orderByExpression, ascending));
        }

        /// <summary>
        /// Applies pagination to the specification.
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// Enables no tracking for the query.
        /// </summary>
        protected virtual void EnableNoTracking()
        {
            IsNoTrackingEnabled = true;
        }

        /// <summary>
        /// Handles the specification logic using the provided context.
        /// This method applies all filters, includes, ordering, and pagination.
        /// </summary>
        /// <param name="context">The specification context</param>
        public virtual void Handle(ISpecificationContext<T> context)
        {
            // Apply no tracking if enabled
            if (IsNoTrackingEnabled)
            {
                context.SetNoTracking();
            }

            // Apply filters
            foreach (var filter in Filters)
            {
                context.AddFilter(filter);
            }

            // Apply includes
            foreach (var include in Includes)
            {
                context.AddInclude(include);
            }

            foreach (var includeString in IncludeStrings)
            {
                context.AddInclude(includeString);
            }

            // Apply ordering
            for (int i = 0; i < OrderByExpressions.Count; i++)
            {
                var orderBy = OrderByExpressions[i];
                if (i == 0)
                {
                    context.AddOrderBy(orderBy.Expression, orderBy.Ascending);
                }
                else
                {
                    context.AddThenOrderBy(orderBy.Expression, orderBy.Ascending);
                }
            }

            // Apply pagination
            if (IsPagingEnabled)
            {
                context.AddPagination(Skip, Take);
            }
        }
    }

    /// <summary>
    /// Base paginated specification implementation.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public abstract class BasePaginatedSpecification<T> : BaseSpecification<T>, IPaginatedSpecification<T>
    {
        public int Skip { get; protected set; }
        public int Take { get; protected set; }

        protected BasePaginatedSpecification(int skip, int take)
        {
            Skip = skip;
            Take = take;
            ApplyPaging(skip, take);
        }
    }

    /// <summary>
    /// Base sorted specification implementation.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public abstract class BaseSortedSpecification<T> : BasePaginatedSpecification<T>, ISortedSpecification<T>
    {
        public int SortedBy { get; protected set; }
        public int SortedDirection { get; protected set; }

        protected BaseSortedSpecification(int skip, int take, int sortedBy, int sortedDirection) 
            : base(skip, take)
        {
            SortedBy = sortedBy;
            SortedDirection = sortedDirection;
        }
    }

    /// <summary>
    /// Base filterable specification implementation.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public abstract class BaseFilterableSpecification<T> : BaseSortedSpecification<T>, IFilterableSpecification<T>
    {
        public string Search { get; protected set; }

        protected BaseFilterableSpecification(int skip, int take, int sortedBy, int sortedDirection, string search) 
            : base(skip, take, sortedBy, sortedDirection)
        {
            Search = search;
        }
    }
} 