using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Specifications.Internal;

/// <summary>
/// Represents a node in the criteria expression tree.
/// Each node can convert itself to an Expression body using a shared parameter.
/// </summary>
internal interface ICriteriaNode<T>
{
    /// <summary>
    /// Converts this node to an Expression body using the provided parameter.
    /// </summary>
    Expression ToExpressionBody(ParameterExpression param);
}

