using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Composite specification that combines two specifications with AND logic.
/// Immutable - returns a new specification when combined.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class AndSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Gets the left specification.
    /// </summary>
    public ISpecification<T> Left { get; }

    /// <summary>
    /// Gets the right specification.
    /// </summary>
    public ISpecification<T> Right { get; }

    /// <summary>
    /// Initializes a new instance of the AndSpecification class.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <exception cref="ArgumentNullException">Thrown when left or right is null.</exception>
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Gets the combined criteria expression using AND logic.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria
    {
        get
        {
            var leftExpr = Left.Criteria;
            var rightExpr = Right.Criteria;

            // Handle null cases
            if (leftExpr is null && rightExpr is null)
                return null;

            if (leftExpr is null)
                return rightExpr;

            if (rightExpr is null)
                return leftExpr;

            // Combine expressions with shared parameter
            var param = Expression.Parameter(typeof(T), "x");
            var leftBody = ParameterReplacer.Replace(leftExpr.Parameters[0], param, leftExpr.Body);
            var rightBody = ParameterReplacer.Replace(rightExpr.Parameters[0], param, rightExpr.Body);
            var body = Expression.AndAlso(leftBody, rightBody);

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    /// <summary>
    /// Determines whether a candidate object satisfies both specifications.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate satisfies both specifications; otherwise, false.</returns>
    public bool IsSatisfiedBy(T candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        return Left.IsSatisfiedBy(candidate) && Right.IsSatisfiedBy(candidate);
    }

    /// <summary>
    /// Converts the specification to an expression tree.
    /// </summary>
    /// <returns>The combined criteria expression, or null if no criteria are defined.</returns>
    public Expression<Func<T, bool>>? ToExpression() => Criteria;
}

/// <summary>
/// Composite specification that combines two specifications with OR logic.
/// Immutable - returns a new specification when combined.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class OrSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Gets the left specification.
    /// </summary>
    public ISpecification<T> Left { get; }

    /// <summary>
    /// Gets the right specification.
    /// </summary>
    public ISpecification<T> Right { get; }

    /// <summary>
    /// Initializes a new instance of the OrSpecification class.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <exception cref="ArgumentNullException">Thrown when left or right is null.</exception>
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Gets the combined criteria expression using OR logic.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria
    {
        get
        {
            var leftExpr = Left.Criteria;
            var rightExpr = Right.Criteria;

            // Handle null cases
            if (leftExpr is null && rightExpr is null)
                return null;

            if (leftExpr is null)
                return rightExpr;

            if (rightExpr is null)
                return leftExpr;

            // Combine expressions with shared parameter
            var param = Expression.Parameter(typeof(T), "x");
            var leftBody = ParameterReplacer.Replace(leftExpr.Parameters[0], param, leftExpr.Body);
            var rightBody = ParameterReplacer.Replace(rightExpr.Parameters[0], param, rightExpr.Body);
            var body = Expression.OrElse(leftBody, rightBody);

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    /// <summary>
    /// Determines whether a candidate object satisfies either specification.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate satisfies either specification; otherwise, false.</returns>
    public bool IsSatisfiedBy(T candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        return Left.IsSatisfiedBy(candidate) || Right.IsSatisfiedBy(candidate);
    }

    /// <summary>
    /// Converts the specification to an expression tree.
    /// </summary>
    /// <returns>The combined criteria expression, or null if no criteria are defined.</returns>
    public Expression<Func<T, bool>>? ToExpression() => Criteria;
}

/// <summary>
/// Composite specification that negates another specification.
/// Immutable - returns a new specification when negated.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public sealed class NotSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Gets the inner specification to negate.
    /// </summary>
    public ISpecification<T> Inner { get; }

    /// <summary>
    /// Initializes a new instance of the NotSpecification class.
    /// </summary>
    /// <param name="inner">The specification to negate.</param>
    /// <exception cref="ArgumentNullException">Thrown when inner is null.</exception>
    public NotSpecification(ISpecification<T> inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Gets the negated criteria expression.
    /// </summary>
    public Expression<Func<T, bool>>? Criteria
    {
        get
        {
            var innerExpr = Inner.Criteria;
            if (innerExpr is null)
                return null;

            var param = Expression.Parameter(typeof(T), "x");
            var innerBody = ParameterReplacer.Replace(innerExpr.Parameters[0], param, innerExpr.Body);
            var body = Expression.Not(innerBody);

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }

    /// <summary>
    /// Determines whether a candidate object does NOT satisfy the inner specification.
    /// </summary>
    /// <param name="candidate">The candidate object to evaluate.</param>
    /// <returns>True if the candidate does not satisfy the inner specification; otherwise, false.</returns>
    public bool IsSatisfiedBy(T candidate)
    {
        if (candidate is null)
            throw new ArgumentNullException(nameof(candidate));

        return !Inner.IsSatisfiedBy(candidate);
    }

    /// <summary>
    /// Converts the specification to an expression tree.
    /// </summary>
    /// <returns>The negated criteria expression, or null if no criteria are defined.</returns>
    public Expression<Func<T, bool>>? ToExpression() => Criteria;
}

/// <summary>
/// Helper class for replacing parameter expressions when combining specifications.
/// </summary>
internal static class ParameterReplacer
{
    /// <summary>
    /// Replaces all occurrences of one parameter with another in an expression body.
    /// </summary>
    /// <param name="from">The parameter to replace.</param>
    /// <param name="to">The parameter to replace with.</param>
    /// <param name="body">The expression body.</param>
    /// <returns>The expression body with replaced parameters.</returns>
    public static Expression Replace(ParameterExpression from, ParameterExpression to, Expression body)
    {
        if (from is null)
            throw new ArgumentNullException(nameof(from));
        if (to is null)
            throw new ArgumentNullException(nameof(to));
        if (body is null)
            throw new ArgumentNullException(nameof(body));

        return new ParameterReplacerVisitor(from, to).Visit(body)!;
    }

    private sealed class ParameterReplacerVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplacerVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from ?? throw new ArgumentNullException(nameof(from));
            _to = to ?? throw new ArgumentNullException(nameof(to));
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _from ? _to : base.VisitParameter(node);
    }
}

