using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Represents a leaf node containing a predicate expression.
/// </summary>
internal sealed class PredicateNode<T> : ICriteriaNode<T>
{
    private readonly Expression<Func<T, bool>> _expr;

    public PredicateNode(Expression<Func<T, bool>> expr)
    {
        _expr = expr ?? throw new ArgumentNullException(nameof(expr));
    }

    public Expression ToExpressionBody(ParameterExpression param)
    {
        return ParameterReplacer.Replace(_expr.Parameters[0], param, _expr.Body);
    }
}

