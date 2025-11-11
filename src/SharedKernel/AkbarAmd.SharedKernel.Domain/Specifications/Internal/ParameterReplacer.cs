using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Expression visitor that replaces parameter expressions in an expression tree.
/// Used to unify parameters when combining multiple expressions.
/// </summary>
internal sealed class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _from;
    private readonly ParameterExpression _to;

    private ParameterReplacer(ParameterExpression from, ParameterExpression to)
    {
        _from = from ?? throw new ArgumentNullException(nameof(from));
        _to = to ?? throw new ArgumentNullException(nameof(to));
    }

    public static Expression Replace(ParameterExpression from, ParameterExpression to, Expression body)
    {
        return new ParameterReplacer(from, to).Visit(body)!;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _from ? _to : base.VisitParameter(node);
    }
}

