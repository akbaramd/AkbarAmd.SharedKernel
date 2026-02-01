// File: ServiceResult.Paging.Partial.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides factory methods for creating paginated service results.
/// </summary>
public sealed partial class ServiceResult
{
    /// <summary>
    /// Creates a successful paginated result with the specified items and pagination information.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the paginated collection.</typeparam>
    /// <param name="items">The items for the current page.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>
    /// A successful result containing a <see cref="ServiceResultPaged{TItem}"/> with the pagination information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    public static ServiceResult<ServiceResultPaged<TItem>> OkPaged<TItem>(
        IEnumerable<TItem> items,
        int pageNumber,
        int pageSize,
        long totalCount,
        string? message = null)
        => ServiceResult<ServiceResultPaged<TItem>>.Ok(
            ServiceResultPaged<TItem>.Create(items, pageNumber, pageSize, totalCount),
            message);

    /// <summary>
    /// Creates a successful paginated result representing an empty page.
    /// </summary>
    /// <typeparam name="TItem">The type of items in the paginated collection.</typeparam>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages. Defaults to 0.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>
    /// A successful result containing an empty <see cref="ServiceResultPaged{TItem}"/>.
    /// </returns>
    public static ServiceResult<ServiceResultPaged<TItem>> EmptyPage<TItem>(
        int pageNumber,
        int pageSize,
        long totalCount = 0,
        string? message = null)
        => ServiceResult<ServiceResultPaged<TItem>>.Ok(
            ServiceResultPaged<TItem>.Empty(pageNumber, pageSize, totalCount),
            message);
}

/// <summary>
/// Represents a paginated collection of items with pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <param name="Items">The items for the current page.</param>
/// <param name="PageNumber">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <remarks>
/// This record provides pagination information along with the items, enabling clients to
/// implement pagination controls and navigation.
/// </remarks>
public sealed record ServiceResultPaged<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount)
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    /// <value>
    /// The total number of pages, calculated as the ceiling of <see cref="TotalCount"/> divided by <see cref="PageSize"/>.
    /// Returns 0 if <see cref="PageSize"/> is less than or equal to 0.
    /// </value>
    public long TotalPages => PageSize <= 0 ? 0 : (long)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    /// <value><c>true</c> if <see cref="PageNumber"/> is greater than 1; otherwise, <c>false</c>.</value>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    /// <value>
    /// <c>true</c> if <see cref="PageNumber"/> is less than <see cref="TotalPages"/>; otherwise, <c>false</c>.
    /// </value>
    public bool HasNext => PageNumber < TotalPages;

    /// <summary>
    /// Creates a paginated result from a collection of items and pagination parameters.
    /// </summary>
    /// <param name="items">The items for the current page.</param>
    /// <param name="pageNumber">The current page number (1-based). Must be greater than or equal to 1.</param>
    /// <param name="pageSize">The number of items per page. Must be greater than or equal to 1.</param>
    /// <param name="totalCount">The total number of items across all pages. Must be greater than or equal to 0.</param>
    /// <returns>A new <see cref="ServiceResultPaged{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than 1,
    /// or when <paramref name="totalCount"/> is less than 0.
    /// </exception>
    public static ServiceResultPaged<T> Create(IEnumerable<T> items, int pageNumber, int pageSize, long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (pageNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber must be greater than or equal to 1.");

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize must be greater than or equal to 1.");

        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount), totalCount, "TotalCount must be greater than or equal to 0.");

        var list = items as IReadOnlyList<T> ?? items.ToArray();
        return new ServiceResultPaged<T>(list, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Creates an empty paginated result.
    /// </summary>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalCount">The total number of items across all pages. Defaults to 0.</param>
    /// <returns>A new <see cref="ServiceResultPaged{T}"/> instance with an empty items collection.</returns>
    public static ServiceResultPaged<T> Empty(int pageNumber, int pageSize, long totalCount = 0)
        => Create(Array.Empty<T>(), pageNumber, pageSize, totalCount);
}
