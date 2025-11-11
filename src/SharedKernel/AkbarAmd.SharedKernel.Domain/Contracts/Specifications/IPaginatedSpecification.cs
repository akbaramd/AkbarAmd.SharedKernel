namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications
{
    /// <summary>
    /// Pagination-aware specification that defines paging behavior.
    /// </summary>
    public interface IPaginatedSpecification<T> : ISpecification<T>
    {
        int PageNumber { get; }
        int PageSize { get; }
    }
}