namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Defines a specification that supports both pagination and sorting.
/// Combines IPaginatedSpecification and ISortSpecification.
/// </summary>
public interface IPaginatedSortableSpecification<T> 
    : IPaginatedSpecification<T>, ISortSpecification<T>
{
}

