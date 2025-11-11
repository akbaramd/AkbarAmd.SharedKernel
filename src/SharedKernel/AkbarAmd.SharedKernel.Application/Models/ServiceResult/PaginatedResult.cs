namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Result for paginated operations with data and pagination metadata
/// </summary>
/// <typeparam name="T">Type of the data items</typeparam>
public class PaginatedResult<T> : ServiceResult<IEnumerable<T>>
{
    public int PageNumber { get; protected set; }
    public int PageSize { get; protected set; }
    public int TotalCount { get; protected set; }
    public int TotalPages { get; protected set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    protected PaginatedResult() : base() { }

    protected PaginatedResult(bool isSuccess, IEnumerable<T>? data = null, string? errorMessage = null, string? errorCode = null,
        int pageNumber = 1, int pageSize = 10, int totalCount = 0) 
        : base(isSuccess, data, errorMessage, errorCode)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    }

    /// <summary>
    /// Creates a successful paginated result
    /// </summary>
    public static new PaginatedResult<T> Success(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>(true, data, null, null, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Creates a successful paginated result with metadata
    /// </summary>
    public static PaginatedResult<T> Success(IEnumerable<T> data, int pageNumber, int pageSize, int totalCount, 
        IDictionary<string, object> metadata)
    {
        var result = new PaginatedResult<T>(true, data, null, null, pageNumber, pageSize, totalCount);
        if (metadata != null)
        {
            foreach (var item in metadata)
            {
                result.WithMetadata(item.Key, item.Value);
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a failed paginated result
    /// </summary>
    public static new PaginatedResult<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new PaginatedResult<T>(false, null, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates a failed paginated result with error code
    /// </summary>
    public static new PaginatedResult<T> FailureWithCode(string errorCode, string errorMessage)
    {
        return new PaginatedResult<T>(false, null, errorMessage, errorCode);
    }

    /// <summary>
    /// Creates an empty paginated result (no data but successful)
    /// </summary>
    public static PaginatedResult<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedResult<T>(true, Enumerable.Empty<T>(), null, null, pageNumber, pageSize, 0);
    }

    /// <summary>
    /// Adds metadata to the result
    /// </summary>
    public new PaginatedResult<T> WithMetadata(string key, object value)
    {
        base.WithMetadata(key, value);
        return this;
    }

    /// <summary>
    /// Adds multiple metadata items to the result
    /// </summary>
    public new PaginatedResult<T> WithMetadata(IDictionary<string, object> metadata)
    {
        base.WithMetadata(metadata);
        return this;
    }

    /// <summary>
    /// Implicit conversion from bool to PaginatedResult<T>
    /// </summary>
    public static implicit operator PaginatedResult<T>(bool isSuccess) => 
        isSuccess ? Empty() : Failure("Operation failed");

    /// <summary>
    /// Implicit conversion from string to PaginatedResult<T> (treats as error message)
    /// </summary>
    public static implicit operator PaginatedResult<T>(string errorMessage) => 
        Failure(errorMessage);

    /// <summary>
    /// Implicit conversion from Exception to PaginatedResult<T>
    /// </summary>
    public static implicit operator PaginatedResult<T>(Exception exception) => 
        Failure(exception.Message, "EXCEPTION");
}
