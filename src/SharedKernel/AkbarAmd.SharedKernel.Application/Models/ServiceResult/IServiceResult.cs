namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Base interface for service results
/// </summary>
public interface IServiceResult
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    bool IsSuccess { get; }
    
    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    string? ErrorMessage { get; }
    
    /// <summary>
    /// Error code for categorization
    /// </summary>
    string? ErrorCode { get; }
    
    /// <summary>
    /// Additional metadata about the result
    /// </summary>
    IDictionary<string, object>? Metadata { get; }
}

/// <summary>
/// Generic interface for service results with data
/// </summary>
/// <typeparam name="T">Type of the data returned</typeparam>
public interface IServiceResult<T> : IServiceResult
{
    /// <summary>
    /// The data returned by the operation
    /// </summary>
    T? Data { get; }
}
