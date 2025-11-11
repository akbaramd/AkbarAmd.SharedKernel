using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Represents an AND logical operation between two criteria nodes.
/// </summary>
internal sealed class AndNode<T> : ICriteriaNode<T>
{
    public ICriteriaNode<T> Left { get; }
    public ICriteriaNode<T> Right { get; }

    public AndNode(ICriteriaNode<T> left, ICriteriaNode<T> right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public Expression ToExpressionBody(ParameterExpression param)
    {
        return Expression.AndAlso(
            Left.ToExpressionBody(param),
            Right.ToExpressionBody(param));
    }
}

