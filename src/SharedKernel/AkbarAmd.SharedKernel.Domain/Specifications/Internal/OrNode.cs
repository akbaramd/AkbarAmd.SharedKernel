using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Represents an OR logical operation between two criteria nodes.
/// </summary>
internal sealed class OrNode<T> : ICriteriaNode<T>
{
    public ICriteriaNode<T> Left { get; }
    public ICriteriaNode<T> Right { get; }

    public OrNode(ICriteriaNode<T> left, ICriteriaNode<T> right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public Expression ToExpressionBody(ParameterExpression param)
    {
        return Expression.OrElse(
            Left.ToExpressionBody(param),
            Right.ToExpressionBody(param));
    }
}

