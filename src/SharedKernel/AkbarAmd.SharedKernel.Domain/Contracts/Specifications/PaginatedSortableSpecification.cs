using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Base specification that supports both pagination and sorting.
/// Paging is handled internally; sorting is controlled through constructor.
/// </summary>
public abstract class PaginatedSortableSpecification<T> 
    : PaginatedSpecification<T>, IPaginatedSortableSpecification<T>
{
    public Expression<Func<T, object>>? SortBy { get; }
    public SortDirection Direction { get; }

    protected PaginatedSortableSpecification(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, object>>? sortBy = null, 
        SortDirection direction = SortDirection.Ascending)
        : base(pageNumber, pageSize)
    {
        SortBy = sortBy;
        Direction = direction;
    }
}