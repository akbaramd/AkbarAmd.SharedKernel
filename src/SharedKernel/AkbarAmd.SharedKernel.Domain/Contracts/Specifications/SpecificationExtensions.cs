using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Extension methods for combining multiple specifications using AND, OR, and NOT operations.
/// This enables the Composite Specification pattern, allowing you to combine multiple
/// ISpecification instances into more complex specifications.
/// 
/// All operations are immutable - they return new specifications without modifying the originals.
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Combines two specifications with AND logic.
    /// Returns a new specification that is satisfied when both specifications are satisfied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The first specification.</param>
    /// <param name="right">The second specification.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when left or right is null.</exception>
    /// <example>
    /// <code>
    /// var activeSpec = new ActiveProductsSpecification();
    /// var categorySpec = new ProductsByCategorySpecification("Electronics");
    /// var combined = activeSpec.And(categorySpec);
    /// </code>
    /// </example>
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));

        return new AndSpecification<T>(left, right);
    }

    /// <summary>
    /// Combines two specifications with OR logic.
    /// Returns a new specification that is satisfied when either specification is satisfied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="left">The first specification.</param>
    /// <param name="right">The second specification.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when left or right is null.</exception>
    /// <example>
    /// <code>
    /// var electronicsSpec = new ProductsByCategorySpecification("Electronics");
    /// var furnitureSpec = new ProductsByCategorySpecification("Furniture");
    /// var combined = electronicsSpec.Or(furnitureSpec);
    /// </code>
    /// </example>
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));

        return new OrSpecification<T>(left, right);
    }

    /// <summary>
    /// Negates a specification.
    /// Returns a new specification that is satisfied when the original specification is not satisfied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specification">The specification to negate.</param>
    /// <returns>A new negated specification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when specification is null.</exception>
    /// <example>
    /// <code>
    /// var activeSpec = new ActiveProductsSpecification();
    /// var inactiveSpec = activeSpec.Not();
    /// </code>
    /// </example>
    public static ISpecification<T> Not<T>(this ISpecification<T> specification)
    {
        if (specification is null)
            throw new ArgumentNullException(nameof(specification));

        return new NotSpecification<T>(specification);
    }

    /// <summary>
    /// Combines a specification with an expression using AND logic.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specification">The specification.</param>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when specification or expression is null.</exception>
    /// <example>
    /// <code>
    /// var activeSpec = new ActiveProductsSpecification();
    /// var combined = activeSpec.And(p => p.Price > 100m);
    /// </code>
    /// </example>
    public static ISpecification<T> And<T>(this ISpecification<T> specification, Expression<Func<T, bool>> expression)
    {
        if (specification is null)
            throw new ArgumentNullException(nameof(specification));
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var expressionSpec = new ExpressionSpecification<T>(expression);
        return new AndSpecification<T>(specification, expressionSpec);
    }

    /// <summary>
    /// Combines a specification with an expression using OR logic.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specification">The specification.</param>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when specification or expression is null.</exception>
    /// <example>
    /// <code>
    /// var activeSpec = new ActiveProductsSpecification();
    /// var combined = activeSpec.Or(p => p.Category == "Furniture");
    /// </code>
    /// </example>
    public static ISpecification<T> Or<T>(this ISpecification<T> specification, Expression<Func<T, bool>> expression)
    {
        if (specification is null)
            throw new ArgumentNullException(nameof(specification));
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var expressionSpec = new ExpressionSpecification<T>(expression);
        return new OrSpecification<T>(specification, expressionSpec);
    }

    /// <summary>
    /// Combines multiple specifications with AND logic.
    /// Returns a new specification that is satisfied when all specifications are satisfied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specifications">The specifications to combine.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentException">Thrown when specifications is null or empty.</exception>
    /// <example>
    /// <code>
    /// var all = SpecificationExtensions.AllOf(
    ///     new ActiveProductsSpecification(),
    ///     new ProductsByCategorySpecification("Electronics"),
    ///     new ProductsByPriceRangeSpecification(100m, 200m)
    /// );
    /// </code>
    /// </example>
    public static ISpecification<T> AllOf<T>(params ISpecification<T>[] specifications)
    {
        if (specifications is null || specifications.Length == 0)
            throw new ArgumentException("At least one specification is required.", nameof(specifications));

        if (specifications.Length == 1)
            return specifications[0];

        ISpecification<T> current = specifications[0];

        for (int i = 1; i < specifications.Length; i++)
        {
            if (specifications[i] is null)
                throw new ArgumentException($"Specification at index {i} is null.", nameof(specifications));

            current = current.And(specifications[i]);
        }

        return current;
    }

    /// <summary>
    /// Combines multiple specifications with OR logic.
    /// Returns a new specification that is satisfied when any specification is satisfied.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="specifications">The specifications to combine.</param>
    /// <returns>A new combined specification.</returns>
    /// <exception cref="ArgumentException">Thrown when specifications is null or empty.</exception>
    /// <example>
    /// <code>
    /// var any = SpecificationExtensions.AnyOf(
    ///     new ProductsByCategorySpecification("Electronics"),
    ///     new ProductsByCategorySpecification("Furniture"),
    ///     new ProductsByPriceRangeSpecification(1000m, 2000m)
    /// );
    /// </code>
    /// </example>
    public static ISpecification<T> AnyOf<T>(params ISpecification<T>[] specifications)
    {
        if (specifications is null || specifications.Length == 0)
            throw new ArgumentException("At least one specification is required.", nameof(specifications));

        if (specifications.Length == 1)
            return specifications[0];

        ISpecification<T> current = specifications[0];

        for (int i = 1; i < specifications.Length; i++)
        {
            if (specifications[i] is null)
                throw new ArgumentException($"Specification at index {i} is null.", nameof(specifications));

            current = current.Or(specifications[i]);
        }

        return current;
    }

    /// <summary>
    /// Internal helper specification that wraps an expression.
    /// </summary>
    private sealed class ExpressionSpecification<T> : ISpecification<T>
    {
        private readonly Expression<Func<T, bool>> _expression;
        private Func<T, bool>? _compiled;

        public ExpressionSpecification(Expression<Func<T, bool>> expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Expression<Func<T, bool>>? Criteria => _expression;

        public bool IsSatisfiedBy(T candidate)
        {
            if (candidate is null)
                throw new ArgumentNullException(nameof(candidate));

            _compiled ??= _expression.Compile();
            return _compiled(candidate);
        }

        public Expression<Func<T, bool>>? ToExpression() => _expression;
    }
}
