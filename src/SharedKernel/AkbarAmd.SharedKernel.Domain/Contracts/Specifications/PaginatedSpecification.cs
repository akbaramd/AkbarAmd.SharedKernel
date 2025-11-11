namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications
{
    /// <summary>
    /// Base class for paginated specifications.
    /// Automatically applies Skip/Take to the underlying query.
    /// </summary>
    public abstract class PaginatedSpecification<T> : BaseSpecification<T>, IPaginatedSpecification<T>
    {
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }

        protected PaginatedSpecification(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than or equal to 1.");

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than or equal to 1.");

            PageNumber = pageNumber;
            PageSize = pageSize;

            // Auto-apply EF Core paging in evaluator
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}