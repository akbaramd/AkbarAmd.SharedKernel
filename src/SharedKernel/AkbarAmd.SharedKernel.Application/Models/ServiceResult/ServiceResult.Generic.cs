// File: ServiceResult.Generic.cs
#nullable enable
using System.Diagnostics;

namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Internal interface for accessing the untyped value of a generic service result.
/// </summary>
/// <remarks>
/// This interface enables type-erased access to the value for scenarios where the generic type
/// is not known at compile time, such as serialization or reflection-based operations.
/// </remarks>
internal interface IUntypedServiceResult
{
    /// <summary>
    /// Gets the untyped value of the service result.
    /// </summary>
    /// <value>The value as an object, or <c>null</c> if no value is present.</value>
    object? UntypedValue { get; }
}

/// <summary>
/// Represents the result of a service operation with a return value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
/// <remarks>
/// This class extends <see cref="ServiceResultBase"/> to include a typed value, enabling
/// functional programming patterns such as monadic operations (Map, Bind) for composing
/// operations in a type-safe manner.
/// </remarks>
public sealed partial class ServiceResult<T> : ServiceResultBase, IUntypedServiceResult
{
    /// <summary>
    /// Gets the value returned by the operation, if successful.
    /// </summary>
    /// <value>The operation result value, or <c>default(T)</c> if the operation failed.</value>
    public T? Value { get; }

    /// <summary>
    /// Gets the untyped value of the service result.
    /// </summary>
    /// <value>The value as an object, or <c>null</c> if no value is present.</value>
    object? IUntypedServiceResult.UntypedValue => Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResult{T}"/> class.
    /// </summary>
    /// <param name="status">The status of the operation.</param>
    /// <param name="value">The operation result value.</param>
    /// <param name="message">The primary message describing the result.</param>
    /// <param name="errorCode">The error code, if applicable.</param>
    /// <param name="errors">The collection of detailed errors.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <param name="traceId">The trace identifier for correlation.</param>
    /// <param name="timestampUtc">The UTC timestamp when the result was created.</param>
    /// <param name="metadata">Additional metadata associated with the result.</param>
    private ServiceResult(
        ServiceResultStatus status,
        T? value,
        string? message,
        string? errorCode,
        IReadOnlyList<ServiceError> errors,
        Exception? exception,
        string? traceId,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string> metadata)
        : base(status, message, errorCode, errors, exception, traceId, timestampUtc, metadata)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new result with the specified trace ID, preserving all other properties.
    /// </summary>
    /// <param name="traceId">The new trace ID.</param>
    /// <returns>
    /// The same instance if the trace ID is unchanged; otherwise, a new instance with the updated trace ID.
    /// </returns>
    public ServiceResult<T> WithTraceId(string? traceId)
    {
        if (string.Equals(TraceId, traceId, StringComparison.Ordinal))
            return this;

        return new ServiceResult<T>(
            status: Status,
            value: Value,
            message: Message,
            errorCode: ErrorCode,
            errors: Errors,
            exception: Exception,
            traceId: traceId,
            timestampUtc: TimestampUtc,
            metadata: Metadata);
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The operation result value.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Success"/>.</returns>
    public static ServiceResult<T> Ok(T? value, string? message = null)
        => new(
            status: ServiceResultStatus.Success,
            value: value,
            message: message,
            errorCode: null,
            errors: EmptyErrors,
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);

    /// <summary>
    /// Creates a failure result with the specified message and optional error details.
    /// </summary>
    /// <param name="message">The failure message. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code.</param>
    /// <param name="exception">An optional exception that caused the failure.</param>
    /// <param name="errors">An optional collection of detailed errors.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Failure"/>.</returns>
    public static ServiceResult<T> Fail(string message, string? code = null, Exception? exception = null, IEnumerable<ServiceError>? errors = null)
        => FromNonGeneric(ServiceResult.Fail(message, code, exception, errors), value: default);

    /// <summary>
    /// Creates a <see cref="ServiceResult{T}"/> from a non-generic <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <param name="value">The value to associate with the result. Defaults to <c>default(T)</c>.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> with the same properties as the input result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ServiceResult<T> FromNonGeneric(ServiceResult result, T? value = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new(
            status: result.Status,
            value: value,
            message: result.Message,
            errorCode: result.ErrorCode,
            errors: result.Errors,
            exception: result.Exception,
            traceId: result.TraceId,
            timestampUtc: result.TimestampUtc,
            metadata: result.Metadata);
    }

    /// <summary>
    /// Creates a <see cref="ServiceResult{T}"/> from a <see cref="ServiceResultBase"/> instance.
    /// </summary>
    /// <param name="result">The base result to convert.</param>
    /// <param name="value">The value to associate with the result. Defaults to <c>default(T)</c>.</param>
    /// <returns>
    /// The same instance if it's already a <see cref="ServiceResult{T}"/>; otherwise, a new instance with the same properties.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ServiceResult<T> FromBase(ServiceResultBase result, T? value = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result is ServiceResult<T> typedResult)
            return typedResult;

        if (result is ServiceResult serviceResult)
            return FromNonGeneric(serviceResult, value);

        return new(
            status: result.Status,
            value: value,
            message: result.Message,
            errorCode: result.ErrorCode,
            errors: result.Errors,
            exception: result.Exception,
            traceId: result.TraceId,
            timestampUtc: result.TimestampUtc,
            metadata: result.Metadata);
    }

    /// <summary>
    /// Creates a <see cref="ServiceResult{T}"/> from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> representing the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static ServiceResult<T> FromException(Exception exception)
        => FromNonGeneric(ServiceResult.FromException(exception));

    /// <summary>
    /// Attempts to get the value from a successful result.
    /// </summary>
    /// <param name="value">When this method returns, contains the value if the operation was successful; otherwise, <c>default(T)</c>.</param>
    /// <returns><c>true</c> if the operation was successful and the value is available; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(out T? value)
    {
        value = Value;
        return IsSuccess;
    }

    /// <summary>
    /// Transforms the value of a successful result using the specified mapping function.
    /// </summary>
    /// <typeparam name="TOut">The type of the transformed value.</typeparam>
    /// <param name="map">The function to apply to the value if the result is successful.</param>
    /// <returns>
    /// A new <see cref="ServiceResult{TOut}"/> with the transformed value if successful;
    /// otherwise, a failure result with the same error information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="map"/> is null.</exception>
    /// <remarks>
    /// This method implements the functor pattern, allowing transformation of values within
    /// the result context without unwrapping. If the result is a failure, the mapping function
    /// is not called and the error information is preserved.
    /// </remarks>
    public ServiceResult<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        if (!IsSuccess)
            return ServiceResult<TOut>.FromBase(this);

        return ServiceResult<TOut>.Ok(map(Value!), message: Message);
    }

    /// <summary>
    /// Binds a function that returns a <see cref="ServiceResult{TOut}"/> to a successful result.
    /// </summary>
    /// <typeparam name="TOut">The type of the value in the resulting service result.</typeparam>
    /// <param name="bind">The function to apply to the value if the result is successful.</param>
    /// <returns>
    /// The result of applying <paramref name="bind"/> to the value if successful;
    /// otherwise, a failure result with the same error information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="bind"/> is null.</exception>
    /// <remarks>
    /// This method implements the monadic bind operation, allowing chaining of operations that
    /// return service results. If the result is a failure, the bind function is not called
    /// and the error information is preserved.
    /// </remarks>
    public ServiceResult<TOut> Bind<TOut>(Func<T, ServiceResult<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        if (!IsSuccess)
            return ServiceResult<TOut>.FromBase(this);

        return bind(Value!);
    }

    /// <summary>
    /// Creates a new result with the specified message, preserving all other properties.
    /// </summary>
    /// <param name="message">The new message.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> instance with the updated message.</returns>
    public ServiceResult<T> WithMessage(string? message)
        => new(Status, Value, message, ErrorCode, Errors, Exception, TraceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified value, preserving all other properties.
    /// </summary>
    /// <param name="value">The new value.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> instance with the updated value.</returns>
    public ServiceResult<T> WithValue(T? value)
        => new(Status, value, Message, ErrorCode, Errors, Exception, TraceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified metadata, preserving all other properties.
    /// </summary>
    /// <param name="metadata">The new metadata dictionary.</param>
    /// <returns>A new <see cref="ServiceResult{T}"/> instance with the updated metadata.</returns>
    public ServiceResult<T> WithMetadata(IReadOnlyDictionary<string, string>? metadata)
        => new(Status, Value, Message, ErrorCode, Errors, Exception, TraceId, TimestampUtc, NormalizeMetadata(metadata));

    // ---------- Conversions ----------

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="ServiceResult{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful result containing the value.</returns>
    public static implicit operator ServiceResult<T>(T value) => Ok(value);

    /// <summary>
    /// Implicitly converts an exception to a <see cref="ServiceResult{T}"/>.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A failure result representing the exception.</returns>
    public static implicit operator ServiceResult<T>(Exception exception) => FromException(exception);

    /// <summary>
    /// Implicitly converts a generic result to a non-generic result, discarding the value.
    /// </summary>
    /// <param name="result">The generic result to convert.</param>
    /// <returns>A non-generic result with the same status and error information.</returns>
    public static implicit operator ServiceResult(ServiceResult<T> result) => ServiceResult.FromBase(result);

    /// <summary>
    /// Implicitly converts a non-generic result to a generic result.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>
    /// A generic result with the same status and error information, with <c>default(T)</c> as the value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="result"/> is successful, as a successful result cannot be
    /// converted without a value. Use <see cref="Ok(T?, string?)"/> or consider using
    /// ServiceResult&lt;Unit&gt; or ServiceResult&lt;object?&gt;.
    /// </exception>
    /// <remarks>
    /// This conversion is intended for failure scenarios where the value type is not relevant.
    /// For successful results, use <see cref="Ok(T?, string?)"/> instead.
    /// </remarks>
    public static implicit operator ServiceResult<T>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Cannot implicitly convert a successful {nameof(ServiceResult)} to {nameof(ServiceResult<T>)} without a value. " +
                $"Return {nameof(ServiceResult<T>)}.{nameof(Ok)}(value), or use {nameof(ServiceResult)}<{nameof(Unit)}> / {nameof(ServiceResult)}<object?>.");
        }

        return FromNonGeneric(result, value: default);
    }

    /// <summary>
    /// Explicitly extracts the value from a successful result.
    /// </summary>
    /// <param name="result">The result to extract the value from.</param>
    /// <returns>The value if the operation was successful.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the operation was not successful. The exception from the result is thrown if available.
    /// </exception>
    /// <remarks>
    /// This explicit conversion should be used with caution. Prefer using <see cref="TryGetValue(out T?)"/>
    /// or pattern matching to safely handle both success and failure cases.
    /// </remarks>
    public static explicit operator T(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccess)
            throw result.Exception ?? new InvalidOperationException(result.Message ?? "Operation failed.");

        return result.Value!;
    }

    /// <summary>
    /// Gets the current trace ID from the activity context, if available.
    /// </summary>
    /// <returns>The trace ID as a string, or <c>null</c> if no activity is available.</returns>
    private static string? GetCurrentTraceId() => Activity.Current?.TraceId.ToString();
}
