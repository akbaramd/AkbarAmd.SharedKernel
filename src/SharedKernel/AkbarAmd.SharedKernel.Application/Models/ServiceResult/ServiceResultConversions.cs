namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Conversion operators for ServiceResult types
/// </summary>
public static partial class ServiceResultConversions
{
    /// <summary>
    /// Converts a non-generic ServiceResult to a generic ServiceResult<T> with default data
    /// </summary>
    public static ServiceResult<T> ToGeneric<T>(this ServiceResult result)
    {
        if (result.IsSuccess)
            return ServiceResult<T>.Success(default(T)!);
        else
            return ServiceResult<T>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorCode);
    }

    /// <summary>
    /// Converts a non-generic ServiceResult to a generic ServiceResult<T> with specified data
    /// </summary>
    public static ServiceResult<T> ToGeneric<T>(this ServiceResult result, T data)
    {
        if (result.IsSuccess)
            return ServiceResult<T>.Success(data);
        else
            return ServiceResult<T>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorCode);
    }

    /// <summary>
    /// Converts a generic ServiceResult<T> to a non-generic ServiceResult
    /// </summary>
    public static ServiceResult ToNonGeneric<T>(this ServiceResult<T> result)
    {
        return result.IsSuccess ? ServiceResult.Success() : ServiceResult.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorCode);
    }

    /// <summary>
    /// Creates a generic result from a non-generic result with data
    /// </summary>
    public static ServiceResult<T> WithData<T>(this ServiceResult result, T data)
    {
        if (result.IsSuccess)
            return ServiceResult<T>.Success(data);
        else
            return ServiceResult<T>.Failure(result.ErrorMessage ?? "Operation failed", result.ErrorCode);
    }

    /// <summary>
    /// Creates a generic result from a non-generic result with data and metadata
    /// </summary>
    public static ServiceResult<T> WithData<T>(this ServiceResult result, T data, IDictionary<string, object> metadata)
    {
        var genericResult = result.WithData(data);
        if (result.Metadata != null)
        {
            foreach (var item in result.Metadata)
            {
                genericResult.WithMetadata(item.Key, item.Value);
            }
        }
        foreach (var item in metadata)
        {
            genericResult.WithMetadata(item.Key, item.Value);
        }
        return genericResult;
    }
}
