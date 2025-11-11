using System.Linq.Expressions;
using System.Reflection;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AkbarAmd.SharedKernel.Infrastructure.Specifications
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SpecificationEvaluationOptions
    {
        public bool AsNoTracking { get; init; } = true;
        public bool UseSplitQuery { get; init; } = false;
        public bool IgnoreAutoIncludes { get; init; } = false;
        public bool IgnoreQueryFilters { get; init; } = false;
        public bool StableSortByIdWhenMissing { get; init; } = true;
        public bool UseIdentityResolutionWhenNoTracking { get; init; } = false;
        public string? QueryTag { get; init; } = null;
    }

    public static class EfCoreSpecificationEvaluator<T> where T : class
    {
        // Backward-compatible overload
        public static IQueryable<T> GetQuery(
            IQueryable<T> inputQuery,
            ISpecification<T> spec,
            bool asNoTracking)
            => GetQuery(inputQuery, spec, new SpecificationEvaluationOptions { AsNoTracking = asNoTracking });

        public static IQueryable<T> GetQuery(
            IQueryable<T> inputQuery,
            ISpecification<T> spec,
            SpecificationEvaluationOptions? options = null)
        {
            if (inputQuery == null) throw new ArgumentNullException(nameof(inputQuery));
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            options ??= new SpecificationEvaluationOptions();

            IQueryable<T> query = inputQuery;

            // 1) Tracking/behavior options
            if (options.AsNoTracking)
            {
                query = options.UseIdentityResolutionWhenNoTracking
                    ? query.AsNoTrackingWithIdentityResolution()
                    : query.AsNoTracking();
            }
            if (options.IgnoreQueryFilters)   query = query.IgnoreQueryFilters();
            if (options.IgnoreAutoIncludes)   query = query.IgnoreAutoIncludes();
            if (!string.IsNullOrWhiteSpace(options.QueryTag)) query = query.TagWith(options.QueryTag);

            // 2) Filtering
            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            // 3) Sorting
            bool sorted = false;

            // Priority 1: Legacy single-sort properties (for backward compatibility)
            if (spec.OrderBy != null)
            {
                var orderByExpr = ConvertDecimalToDoubleIfNeeded(spec.OrderBy);
                query = query.OrderBy(orderByExpr);
                sorted = true;
            }
            else if (spec.OrderByDescending != null)
            {
                var orderByDescExpr = ConvertDecimalToDoubleIfNeeded(spec.OrderByDescending);
                query = query.OrderByDescending(orderByDescExpr);
                sorted = true;
            }
            // Priority 2: Multi-level sorting chain
            else if (spec is IMultiSortSpecification<T> multiSort && multiSort.Sorts.Count > 0)
            {
                query = ApplySortChain(query, multiSort.Sorts);
                sorted = true;
            }
            // Priority 3: Legacy ISortSpecification interface
            else if (spec is ISortSpecification<T> sortable && sortable.SortBy != null)
            {
                var sortByExpr = ConvertDecimalToDoubleIfNeeded(sortable.SortBy);
                query = sortable.Direction == SortDirection.Descending
                    ? query.OrderByDescending(sortByExpr)
                    : query.OrderBy(sortByExpr);
                sorted = true;
            }

            // 4) Fallback stable ordering by key if requested and no explicit sort
            if (!sorted && options.StableSortByIdWhenMissing)
            {
                var keySelector = TryBuildDefaultKeySelector();
                if (keySelector != null)
                    query = query.OrderBy(keySelector);
            }

            // 5) Includes (Expressions + Strings)
            if (spec.Includes?.Count > 0)
            {
                foreach (var includeExpression in spec.Includes)
                    query = query.Include(includeExpression);
            }
            if (spec.IncludeStrings?.Count > 0)
            {
                foreach (var includeString in spec.IncludeStrings)
                    query = query.Include(includeString);
            }

            // 6) Split query to avoid cartesian explosion if desired
            if (options.UseSplitQuery)
                query = query.AsSplitQuery();

            // 7) Paging
            if (spec.IsPagingEnabled)
            {
                if (spec.Skip < 0)  throw new ArgumentOutOfRangeException(nameof(spec.Skip), "Skip cannot be negative.");
                if (spec.Take <= 0) throw new ArgumentOutOfRangeException(nameof(spec.Take), "Take must be greater than zero.");

                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }

        // Helpers for Count/Any (filter only, no includes/paging)
        public static int Count(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            if (inputQuery == null) throw new ArgumentNullException(nameof(inputQuery));
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            return spec.Criteria != null ? inputQuery.Count(spec.Criteria) : inputQuery.Count();
        }

        public static bool Any(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            if (inputQuery == null) throw new ArgumentNullException(nameof(inputQuery));
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            return spec.Criteria != null ? inputQuery.Any(spec.Criteria) : inputQuery.Any();
        }

        // ---- internal: build x => x.Id or x => x.{TypeName}Id if exists
        private static Expression<Func<T, object>>? TryBuildDefaultKeySelector()
        {
            var t = typeof(T);
            var idProp = t.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                       ?? t.GetProperty(t.Name + "Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (idProp == null) return null;

            var p = Expression.Parameter(t, "x");
            var body = Expression.Convert(Expression.Property(p, idProp), typeof(object));
            return Expression.Lambda<Func<T, object>>(body, p);
        }

        // ---- Multi-level sorting chain application
        private static IQueryable<T> ApplySortChain(IQueryable<T> query, IReadOnlyList<SortDescriptor<T>> sorts)
        {
            if (sorts.Count == 0)
                return query;

            IOrderedQueryable<T>? ordered = null;

            for (int i = 0; i < sorts.Count; i++)
            {
                var sortDescriptor = sorts[i];
                var keySelector = sortDescriptor.KeySelector;

                // Check if null ordering is needed and applicable
                bool needsNullOrdering = sortDescriptor.Nulls != NullSort.Unspecified && 
                    (IsNullableType(keySelector.Body.Type) || !keySelector.Body.Type.IsValueType);

                if (i == 0)
                {
                    // First sort level
                    if (needsNullOrdering)
                    {
                        // Apply null rank first, then main key
                        var nullRank = BuildNullRank(keySelector, sortDescriptor.Nulls == NullSort.NullsFirst);
                        ordered = query.OrderBy(nullRank);
                        ordered = ApplyThenOrder(ordered, sortDescriptor);
                    }
                    else
                    {
                        // Apply main key only
                        ordered = ApplyFirstOrder(query, sortDescriptor);
                    }
                }
                else
                {
                    // Subsequent sort levels
                    if (needsNullOrdering)
                    {
                        // Apply null rank first, then main key
                        var nullRank = BuildNullRank(keySelector, sortDescriptor.Nulls == NullSort.NullsFirst);
                        ordered = ordered!.ThenBy(nullRank);
                        ordered = ApplyThenOrder(ordered, sortDescriptor);
                    }
                    else
                    {
                        // Apply main key only
                        ordered = ApplyThenOrder(ordered!, sortDescriptor);
                    }
                }
            }

            return ordered ?? query;
        }

        private static IOrderedQueryable<T> ApplyFirstOrder(IQueryable<T> source, SortDescriptor<T> descriptor)
        {
            return descriptor.Direction == SortDirection.Descending
                ? OrderByDynamic(source, descriptor.KeySelector, descending: true)
                : OrderByDynamic(source, descriptor.KeySelector, descending: false);
        }

        private static IOrderedQueryable<T> ApplyThenOrder(IOrderedQueryable<T> source, SortDescriptor<T> descriptor)
        {
            return descriptor.Direction == SortDirection.Descending
                ? ThenByDynamic(source, descriptor.KeySelector, descending: true)
                : ThenByDynamic(source, descriptor.KeySelector, descending: false);
        }

        /// <summary>
        /// Builds a rank expression that returns 0 for nulls (if nullsFirst) or 1 for nulls (if nullsLast).
        /// This allows EF Core to translate null ordering to SQL.
        /// </summary>
        private static Expression<Func<T, int>> BuildNullRank(LambdaExpression keySelector, bool nullsFirst)
        {
            var param = keySelector.Parameters[0];
            var body = keySelector.Body;
            var nullConst = Expression.Constant(null, body.Type);
            var isNull = Expression.Equal(body, nullConst);
            var whenTrue = Expression.Constant(nullsFirst ? 0 : 1);
            var whenFalse = Expression.Constant(nullsFirst ? 1 : 0);
            var condition = Expression.Condition(isNull, whenTrue, whenFalse);
            return Expression.Lambda<Func<T, int>>(condition, param);
        }

        private static bool IsNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Dynamically invokes OrderBy/OrderByDescending with the correct generic type.
        /// </summary>
        private static IOrderedQueryable<T> OrderByDynamic(IQueryable<T> source, LambdaExpression keySelector, bool descending)
        {
            var methodName = descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var method = GetQueryableMethod(methodName);
            var genericMethod = method.MakeGenericMethod(typeof(T), keySelector.Body.Type);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, keySelector })!;
        }

        /// <summary>
        /// Dynamically invokes ThenBy/ThenByDescending with the correct generic type.
        /// </summary>
        private static IOrderedQueryable<T> ThenByDynamic(IOrderedQueryable<T> source, LambdaExpression keySelector, bool descending)
        {
            var methodName = descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);
            var method = GetQueryableMethod(methodName);
            var genericMethod = method.MakeGenericMethod(typeof(T), keySelector.Body.Type);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, keySelector })!;
        }

        private static MethodInfo GetQueryableMethod(string name)
        {
            return typeof(Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == name && m.GetParameters().Length == 2);
        }

        // ---- public: Convert decimal expressions to double for SQLite compatibility
        // SQLite does not support ordering by decimal directly, so we convert to double
        // This method is public so it can be used by repository implementations that apply ordering directly
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
