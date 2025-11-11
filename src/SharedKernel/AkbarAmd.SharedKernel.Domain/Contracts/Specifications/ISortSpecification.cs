using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Defines a specification that supports sorting independently of pagination.
/// </summary>
public interface ISortSpecification<T>
{
    /// <summary>
    /// The property selector used for sorting.
    /// Example: x => x.Name
    /// </summary>
    Expression<Func<T, object>>? SortBy { get; }

    /// <summary>
    /// The direction of sorting (Ascending or Descending).
    /// </summary>
    SortDirection Direction { get; }
}

