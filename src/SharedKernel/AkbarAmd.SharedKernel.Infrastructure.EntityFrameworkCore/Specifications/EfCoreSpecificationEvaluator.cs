using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore.Specifications
{
    /// <summary>
    /// EF Core implementation of ISpecificationEvaluator.
    /// Handles evaluation of domain specifications (criteria only) against EF Core IQueryable.
    /// Includes, sorting, and pagination are handled at the repository level.
    /// </summary>
    public sealed class EfCoreSpecificationEvaluator<T> : ISpecificationEvaluator<T> where T : class
    {
        /// <summary>
        /// Applies a specification to an IQueryable and returns the transformed query.
        /// Only applies criteria - no includes, sorting, or pagination.
        /// </summary>
        public IQueryable<T> Evaluate(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            options ??= new SpecificationEvaluationOptions();

            IQueryable<T> result = query;

            // 1) Tracking/behavior options
            if (options.AsNoTracking)
            {
                result = options.UseIdentityResolutionWhenNoTracking
                    ? result.AsNoTrackingWithIdentityResolution()
                    : result.AsNoTracking();
            }
            if (options.IgnoreQueryFilters)   result = result.IgnoreQueryFilters();
            if (options.IgnoreAutoIncludes)   result = result.IgnoreAutoIncludes();
            if (!string.IsNullOrWhiteSpace(options.QueryTag)) result = result.TagWith(options.QueryTag);

            // 2) Filtering (criteria only)
            if (specification.Criteria != null)
                result = result.Where(specification.Criteria);

            return result;
        }

        /// <summary>
        /// Applies a specification optimized for counting (same as Evaluate since we only have criteria).
        /// </summary>
        public IQueryable<T> EvaluateForCount(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            options ??= new SpecificationEvaluationOptions();

            IQueryable<T> result = query;

            // Apply only filtering (criteria only)
            if (specification.Criteria != null)
                result = result.Where(specification.Criteria);

            return result;
        }

        /// <summary>
        /// Checks if any items match the specification criteria.
        /// </summary>
        public bool Any(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            return specification.Criteria != null ? query.Any(specification.Criteria) : query.Any();
        }

        /// <summary>
        /// Counts items matching the specification criteria.
        /// </summary>
        public int Count(IQueryable<T> query, ISpecification<T> specification, SpecificationEvaluationOptions? options = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            return specification.Criteria != null ? query.Count(specification.Criteria) : query.Count();
        }

        // Backward-compatible static methods that delegate to instance methods
        private static readonly EfCoreSpecificationEvaluator<T> Instance = new();

        public static IQueryable<T> GetQuery(
            IQueryable<T> inputQuery,
            ISpecification<T> spec,
            bool asNoTracking)
            => Instance.Evaluate(inputQuery, spec, new SpecificationEvaluationOptions { AsNoTracking = asNoTracking });

        public static IQueryable<T> GetQuery(
            IQueryable<T> inputQuery,
            ISpecification<T> spec,
            SpecificationEvaluationOptions? options = null)
            => Instance.Evaluate(inputQuery, spec, options);

        public static int Count(IQueryable<T> inputQuery, ISpecification<T> spec)
            => Instance.Count(inputQuery, spec);

        public static bool Any(IQueryable<T> inputQuery, ISpecification<T> spec)
            => Instance.Any(inputQuery, spec);

        /// <summary>
        /// Converts decimal expressions to double for SQLite compatibility.
        /// SQLite does not support ordering by decimal directly, so we convert to double.
        /// This method is public so it can be used by repository implementations that apply ordering directly.
        /// </summary>
        public static Expression<Func<T, object>> ConvertDecimalToDoubleIfNeeded(Expression<Func<T, object>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            // Extract the underlying expression (unwrap Convert to object if present)
            Expression underlyingExpression = expression.Body;
            Type underlyingType = underlyingExpression.Type;

            // If body is a Convert expression (e.g., Convert(p.Price, Object)), get the operand
            if (underlyingExpression is UnaryExpression unary && 
                unary.NodeType == ExpressionType.Convert && 
                unary.Type == typeof(object))
            {
                underlyingExpression = unary.Operand;
                underlyingType = underlyingExpression.Type;
            }

            // Check if the underlying type is decimal or nullable decimal
            if (underlyingType == typeof(decimal) || underlyingType == typeof(decimal?))
            {
                // Convert decimal to double, then to object
                var doubleConversion = Expression.Convert(underlyingExpression, typeof(double));
                var objectConversion = Expression.Convert(doubleConversion, typeof(object));
                return Expression.Lambda<Func<T, object>>(objectConversion, expression.Parameters);
            }

            // Return original expression if not decimal
            return expression;
        }
    }
}
