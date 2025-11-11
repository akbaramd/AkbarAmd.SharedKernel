using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Represents a NOT logical operation on a criteria node.
/// </summary>
internal sealed class NotNode<T> : ICriteriaNode<T>
{
    public ICriteriaNode<T> Inner { get; }

    public NotNode(ICriteriaNode<T> inner)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Expression ToExpressionBody(ParameterExpression param)
    {
        return Expression.Not(Inner.ToExpressionBody(param));
    }
}

