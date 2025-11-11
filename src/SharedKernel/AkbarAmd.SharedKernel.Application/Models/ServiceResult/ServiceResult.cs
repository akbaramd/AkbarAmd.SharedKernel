namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Base implementation of service result without data
/// </summary>
public partial class ServiceResult : IServiceResult
{
    public bool IsSuccess { get; protected set; }
    public string? ErrorMessage { get; protected set; }
    public string? ErrorCode { get; protected set; }
    public IDictionary<string, object>? Metadata { get; protected set; }

    protected ServiceResult()
    {
        Metadata = new Dictionary<string, object>();
    }

    protected ServiceResult(bool isSuccess, string? errorMessage = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static ServiceResult Success() => new(true);

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static ServiceResult Failure(string errorMessage, string? errorCode = null) => 
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result with error code (alternative signature)
    /// </summary>
    public static ServiceResult FailureWithCode(string errorCode, string errorMessage) => 
        new(false, errorMessage, errorCode);

    /// <summary>
    /// Adds metadata to the result
    /// </summary>
    public ServiceResult WithMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple metadata items to the result
    /// </summary>
    public ServiceResult WithMetadata(IDictionary<string, object> metadata)
    {
        Metadata ??= new Dictionary<string, object>();
        foreach (var item in metadata)
        {
            Metadata[item.Key] = item.Value;
        }
        return this;
    }

    /// <summary>
    /// Implicit conversion from bool to ServiceResult
    /// </summary>
    public static implicit operator ServiceResult(bool isSuccess) => 
        isSuccess ? Success() : Failure("Operation failed");

    /// <summary>
    /// Implicit conversion from string to ServiceResult (treats as error message)
    /// </summary>
    public static implicit operator ServiceResult(string errorMessage) => 
        Failure(errorMessage);

    /// <summary>
    /// Implicit conversion from Exception to ServiceResult
    /// </summary>
    public static implicit operator ServiceResult(Exception exception) => 
        Failure(exception.Message, "EXCEPTION");
}
/// <summary>
/// Generic implementation of service result with data
/// </summary>
/// <typeparam name="T">Type of the data returned</typeparam>
public class ServiceResult<T> : ServiceResult, IServiceResult<T>
{
    public T? Data { get; protected set; }

    protected ServiceResult() : base() { }

    protected ServiceResult(bool isSuccess, T? data = default, string? errorMessage = null, string? errorCode = null) 
        : base(isSuccess, errorMessage, errorCode)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> Success(T data) => new(true, data);

    /// <summary>
    /// Creates a successful result with data and metadata
    /// </summary>
    public static ServiceResult<T> Success(T data, IDictionary<string, object> metadata)
    {
        var result = new ServiceResult<T>(true, data);
        result.WithMetadata(metadata);
        return result;
    }

    /// <summary>
    /// Creates a failed result
    /// </summary>
    public static new ServiceResult<T> Failure(string errorMessage, string? errorCode = null) => 
        new(false, default, errorMessage, errorCode);

    /// <summary>
    /// Creates a failed result with error code (alternative signature)
    /// </summary>
    public static new ServiceResult<T> FailureWithCode(string errorCode, string errorMessage) => 
        new(false, default, errorMessage, errorCode);

    /// <summary>
    /// Adds metadata to the result
    /// </summary>
    public new ServiceResult<T> WithMetadata(string key, object value)
    {
        base.WithMetadata(key, value);
        return this;
    }

    /// <summary>
    /// Adds multiple metadata items to the result
    /// </summary>
    public new ServiceResult<T> WithMetadata(IDictionary<string, object> metadata)
    {
        base.WithMetadata(metadata);
        return this;
    }

  

    /// <summary>
    /// Implicit conversion from T to ServiceResult<T>
    /// </summary>
    public static implicit operator ServiceResult<T>(T data) => Success(data);

    /// <summary>
    /// Implicit conversion from bool to ServiceResult<T>
    /// </summary>
    public static implicit operator ServiceResult<T>(bool isSuccess) => 
        isSuccess ? new ServiceResult<T>(true, default) : new ServiceResult<T>(false, default, "Operation failed");

    /// <summary>
    /// Implicit conversion from string to ServiceResult<T> (treats as error message)
    /// </summary>
    public static implicit operator ServiceResult<T>(string errorMessage) => 
        new ServiceResult<T>(false, default, errorMessage);

    /// <summary>
    /// Implicit conversion from Exception to ServiceResult<T>
    /// </summary>
    public static implicit operator ServiceResult<T>(Exception exception) => 
        new ServiceResult<T>(false, default, exception.Message, "EXCEPTION");
}
