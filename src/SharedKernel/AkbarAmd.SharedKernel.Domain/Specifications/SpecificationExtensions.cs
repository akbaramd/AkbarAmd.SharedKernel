using System.Linq.Expressions;
using AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

namespace AkbarAmd.SharedKernel.Domain.Specifications;

/// <summary>
/// Extension methods for combining specifications with AND, OR, and NOT operations.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications with AND logic.
    /// </summary>
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));

        return Combine(left, right, (l, r) => Expression.AndAlso(l, r));
    }

    /// <summary>
    /// Combines two specifications with OR logic.
    /// </summary>
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));

        return Combine(left, right, (l, r) => Expression.OrElse(l, r));
    }

    /// <summary>
    /// Negates a specification with NOT logic.
    /// </summary>
    public static ISpecification<T> Not<T>(this ISpecification<T> spec)
    {
        if (spec is null)
            throw new ArgumentNullException(nameof(spec));
        if (spec.Criteria is null)
            throw new InvalidOperationException("Cannot negate a specification without criteria.");

        var param = spec.Criteria.Parameters[0];
        var body = Expression.Not(spec.Criteria.Body);
        var expr = Expression.Lambda<Func<T, bool>>(body, param);

        return new AdHocSpecification<T>(expr, spec);
    }

    private static ISpecification<T> Combine<T>(
        ISpecification<T> left,
        ISpecification<T> right,
        Func<Expression, Expression, BinaryExpression> merge)
    {
        if (left.Criteria is null)
            throw new InvalidOperationException($"Left specification must have criteria. Type: {typeof(T).Name}");
        if (right.Criteria is null)
            throw new InvalidOperationException($"Right specification must have criteria. Type: {typeof(T).Name}");

        var param = left.Criteria.Parameters[0];
        var rightBody = ParameterReplacer.Replace(right.Criteria.Parameters[0], param, right.Criteria.Body);
        var body = merge(left.Criteria.Body, rightBody);
        var expr = Expression.Lambda<Func<T, bool>>(body, param);

        return new AdHocSpecification<T>(expr, left, right);
    }

    /// <summary>
    /// Ad-hoc specification that wraps a criteria expression and optionally preserves
    /// includes, sorting, and paging from source specifications.
    /// </summary>
    private sealed class AdHocSpecification<T> : BaseSpecification<T>
    {
        public AdHocSpecification(Expression<Func<T, bool>> criteria, params ISpecification<T>[] sources)
        {
            Where(criteria);

            // Preserve includes, sorting, and paging from source specifications
            foreach (var source in sources)
            {
                if (source is null) continue;

                foreach (var include in source.Includes)
                    AddInclude(include);

                foreach (var includeString in source.IncludeStrings)
                    AddInclude(includeString);

                if (source.OrderBy != null)
                    AddOrderBy(source.OrderBy);
                else if (source.OrderByDescending != null)
                    AddOrderByDescending(source.OrderByDescending);

                if (source.IsPagingEnabled)
                    ApplyPaging(source.Skip, source.Take);
            }
        }
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        private ParameterReplacer(ParameterExpression from, ParameterExpression to)
        {
            _from = from ?? throw new ArgumentNullException(nameof(from));
            _to = to ?? throw new ArgumentNullException(nameof(to));
        }

        public static Expression Replace(ParameterExpression from, ParameterExpression to, Expression body)
            => new ParameterReplacer(from, to).Visit(body)!;

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }
}

