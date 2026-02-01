// File: ServiceResult.Core.cs
#nullable enable
using System.Diagnostics;

namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Represents the result of a service operation without a return value.
/// </summary>
/// <remarks>
/// This class provides a standardized way to represent operation outcomes, supporting both
/// success and various failure scenarios. It implements an immutable result pattern that
/// enables functional error handling without exceptions for expected failures.
/// </remarks>
public sealed partial class ServiceResult : ServiceResultBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResult"/> class.
    /// </summary>
    /// <param name="status">The status of the operation.</param>
    /// <param name="message">The primary message describing the result.</param>
    /// <param name="errorCode">The error code, if applicable.</param>
    /// <param name="errors">The collection of detailed errors.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <param name="traceId">The trace identifier for correlation.</param>
    /// <param name="timestampUtc">The UTC timestamp when the result was created.</param>
    /// <param name="metadata">Additional metadata associated with the result.</param>
    private ServiceResult(
        ServiceResultStatus status,
        string? message,
        string? errorCode,
        IReadOnlyList<ServiceError> errors,
        Exception? exception,
        string? traceId,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string> metadata)
        : base(status, message, errorCode, errors, exception, traceId, timestampUtc, metadata)
    {
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="message">An optional success message.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Success"/>.</returns>
    public static ServiceResult Ok(string? message = null)
        => new(
            status: ServiceResultStatus.Success,
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
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Failure"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    public static ServiceResult Fail(
        string message,
        string? code = null,
        Exception? exception = null,
        IEnumerable<ServiceError>? errors = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var normalized = NormalizeErrors(errors);
        if (normalized.Count == 0)
        {
            var error = exception is null
                ? ServiceError.Failure(code ?? "FAILURE", message)
                : ServiceError.Exception(code ?? exception.GetType().Name, message);

            normalized = Array.AsReadOnly(new[] { error });
        }

        return new(
            status: ServiceResultStatus.Failure,
            message: message,
            errorCode: code,
            errors: normalized,
            exception: exception,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a not found result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Not found."</param>
    /// <param name="code">An optional error code. Defaults to "NOT_FOUND".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.NotFound"/>.</returns>
    public static ServiceResult NotFound(string message = "Not found.", string? code = null, string? target = null)
    {
        var errorCode = code ?? "NOT_FOUND";
        return new(
            status: ServiceResultStatus.NotFound,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.NotFound(errorCode, message, target) }),
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates an unauthorized access result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Unauthorized."</param>
    /// <param name="code">An optional error code. Defaults to "UNAUTHORIZED".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Unauthorized"/>.</returns>
    public static ServiceResult Unauthorized(string message = "Unauthorized.", string? code = null, string? target = null)
    {
        var errorCode = code ?? "UNAUTHORIZED";
        return new(
            status: ServiceResultStatus.Unauthorized,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Unauthorized(errorCode, message, target) }),
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a forbidden access result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Forbidden."</param>
    /// <param name="code">An optional error code. Defaults to "FORBIDDEN".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Forbidden"/>.</returns>
    public static ServiceResult Forbidden(string message = "Forbidden.", string? code = null, string? target = null)
    {
        var errorCode = code ?? "FORBIDDEN";
        return new(
            status: ServiceResultStatus.Forbidden,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Forbidden(errorCode, message, target) }),
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a conflict result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Conflict."</param>
    /// <param name="code">An optional error code. Defaults to "CONFLICT".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Conflict"/>.</returns>
    public static ServiceResult Conflict(string message = "Conflict.", string? code = null, string? target = null)
    {
        var errorCode = code ?? "CONFLICT";
        return new(
            status: ServiceResultStatus.Conflict,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Conflict(errorCode, message, target) }),
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates an unavailable service result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Service unavailable."</param>
    /// <param name="code">An optional error code. Defaults to "UNAVAILABLE".</param>
    /// <param name="target">An optional target service identifier.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Unavailable"/>.</returns>
    public static ServiceResult Unavailable(string message = "Service unavailable.", string? code = null, string? target = null)
    {
        var errorCode = code ?? "UNAVAILABLE";
        return new(
            status: ServiceResultStatus.Unavailable,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Unavailable(errorCode, message, target) }),
            exception: null,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a timeout result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Timeout."</param>
    /// <param name="code">An optional error code. Defaults to "TIMEOUT".</param>
    /// <param name="target">An optional target operation identifier.</param>
    /// <param name="exception">An optional timeout exception.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Timeout"/>.</returns>
    public static ServiceResult Timeout(string message = "Timeout.", string? code = null, string? target = null, Exception? exception = null)
    {
        var errorCode = code ?? "TIMEOUT";
        return new(
            status: ServiceResultStatus.Timeout,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Timeout(errorCode, message, target) }),
            exception: exception,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a cancelled operation result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Cancelled."</param>
    /// <param name="code">An optional error code. Defaults to "CANCELLED".</param>
    /// <param name="target">An optional target operation identifier.</param>
    /// <param name="exception">An optional cancellation exception.</param>
    /// <returns>A new <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Cancelled"/>.</returns>
    public static ServiceResult Cancelled(string message = "Cancelled.", string? code = null, string? target = null, Exception? exception = null)
    {
        var errorCode = code ?? "CANCELLED";
        return new(
            status: ServiceResultStatus.Cancelled,
            message: message,
            errorCode: errorCode,
            errors: Array.AsReadOnly(new[] { ServiceError.Cancelled(errorCode, message, target) }),
            exception: exception,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a <see cref="ServiceResult"/> from a <see cref="ServiceResultBase"/> instance.
    /// </summary>
    /// <param name="result">The base result to convert.</param>
    /// <returns>
    /// The same instance if it's already a <see cref="ServiceResult"/>, otherwise a new instance with the same properties.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ServiceResult FromBase(ServiceResultBase result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result is ServiceResult serviceResult)
            return serviceResult;

        return new ServiceResult(
            status: result.Status,
            message: result.Message,
            errorCode: result.ErrorCode,
            errors: result.Errors,
            exception: result.Exception,
            traceId: result.TraceId,
            timestampUtc: result.TimestampUtc,
            metadata: result.Metadata);
    }

    /// <summary>
    /// Creates a <see cref="ServiceResult"/> from an exception, mapping common exception types to appropriate statuses.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>
    /// A <see cref="ServiceResult"/> with an appropriate status based on the exception type.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    /// <remarks>
    /// This method maps common exception types to their corresponding result statuses:
    /// <list type="bullet">
    /// <item><see cref="OperationCanceledException"/> → <see cref="ServiceResultStatus.Cancelled"/></item>
    /// <item><see cref="TimeoutException"/> → <see cref="ServiceResultStatus.Timeout"/></item>
    /// <item><see cref="KeyNotFoundException"/> → <see cref="ServiceResultStatus.NotFound"/></item>
    /// <item><see cref="UnauthorizedAccessException"/> → <see cref="ServiceResultStatus.Unauthorized"/></item>
    /// <item><see cref="ArgumentException"/> → <see cref="ServiceResultStatus.ValidationFailed"/></item>
    /// <item><see cref="AggregateException"/> → Handles multiple exceptions appropriately</item>
    /// </list>
    /// </remarks>
    public static ServiceResult FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Try custom exception mapping first (e.g., FluentValidation)
        if (TryMapException(exception, out var mapped))
            return mapped;

        // Handle aggregate exceptions
        if (exception is AggregateException aggregateException)
        {
            var flattened = aggregateException.Flatten();
            if (flattened.InnerExceptions.Count == 1)
                return FromException(flattened.InnerExceptions[0]);

            var errors = new ServiceError[flattened.InnerExceptions.Count];
            for (int i = 0; i < flattened.InnerExceptions.Count; i++)
            {
                var innerEx = flattened.InnerExceptions[i];
                errors[i] = ServiceError.Exception(innerEx.GetType().Name, innerEx.Message);
            }

            return new(
                status: ServiceResultStatus.Failure,
                message: "Multiple exceptions occurred.",
                errorCode: "AGGREGATE_EXCEPTION",
                errors: Array.AsReadOnly(errors),
                exception: flattened,
                traceId: GetCurrentTraceId(),
                timestampUtc: DateTimeOffset.UtcNow,
                metadata: EmptyMetadata);
        }

        // Map common exception types to appropriate statuses
        return exception switch
        {
            OperationCanceledException => Cancelled(exception.Message, code: exception.GetType().Name, exception: exception),
            TimeoutException => Timeout(exception.Message, code: exception.GetType().Name, exception: exception),
            KeyNotFoundException => NotFound(exception.Message, code: exception.GetType().Name),
            UnauthorizedAccessException => Unauthorized(exception.Message, code: exception.GetType().Name),
            ArgumentException argEx => CreateValidationFailedResult(argEx),
            _ => CreateGenericFailureResult(exception)
        };
    }

    /// <summary>
    /// Creates a new result with the specified message, preserving all other properties.
    /// </summary>
    /// <param name="message">The new message.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the updated message.</returns>
    public ServiceResult WithMessage(string? message)
        => new(Status, message, ErrorCode, Errors, Exception, TraceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified error code, preserving all other properties.
    /// </summary>
    /// <param name="errorCode">The new error code.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the updated error code.</returns>
    public ServiceResult WithErrorCode(string? errorCode)
        => new(Status, Message, errorCode, Errors, Exception, TraceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified trace ID, preserving all other properties.
    /// </summary>
    /// <param name="traceId">The new trace ID.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the updated trace ID.</returns>
    public ServiceResult WithTraceId(string? traceId)
        => new(Status, Message, ErrorCode, Errors, Exception, traceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified exception, preserving all other properties.
    /// </summary>
    /// <param name="exception">The new exception.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the updated exception.</returns>
    public ServiceResult WithException(Exception? exception)
        => new(Status, Message, ErrorCode, Errors, exception, TraceId, TimestampUtc, Metadata);

    /// <summary>
    /// Creates a new result with the specified metadata, preserving all other properties.
    /// </summary>
    /// <param name="metadata">The new metadata dictionary.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the updated metadata.</returns>
    public ServiceResult WithMetadata(IReadOnlyDictionary<string, string>? metadata)
        => new(Status, Message, ErrorCode, Errors, Exception, TraceId, TimestampUtc, NormalizeMetadata(metadata));

    /// <summary>
    /// Creates a new result with an additional metadata entry, preserving all other properties.
    /// </summary>
    /// <param name="key">The metadata key. Must not be null or whitespace.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>A new <see cref="ServiceResult"/> instance with the added metadata entry.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
    public ServiceResult AddMetadata(string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var dictionary = Metadata.Count == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(Metadata, StringComparer.Ordinal);

        dictionary[key] = value;
        return WithMetadata(new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(dictionary));
    }

    /// <summary>
    /// Attempts to map a custom exception type to a <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <param name="result">When this method returns, contains the mapped result if mapping succeeded; otherwise, the default value.</param>
    /// <returns><c>true</c> if the exception was successfully mapped; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This partial method allows derived classes or other partial class definitions to provide custom exception mapping logic.
    /// </remarks>
    private static partial bool TryMapException(Exception exception, out ServiceResult result);

    /// <summary>
    /// Implicitly converts an exception to a <see cref="ServiceResult"/>.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <returns>A <see cref="ServiceResult"/> representing the exception.</returns>
    public static implicit operator ServiceResult(Exception exception) => FromException(exception);

    /// <summary>
    /// Gets the current trace ID from the activity context, if available.
    /// </summary>
    /// <returns>The trace ID as a string, or <c>null</c> if no activity is available.</returns>
    private static string? GetCurrentTraceId() => Activity.Current?.TraceId.ToString();

    /// <summary>
    /// Creates a validation failed result from an <see cref="ArgumentException"/>.
    /// </summary>
    private static ServiceResult CreateValidationFailedResult(ArgumentException argumentException)
    {
        var error = ServiceError.Validation(
            argumentException.GetType().Name,
            argumentException.Message,
            target: argumentException.ParamName);

        return new(
            status: ServiceResultStatus.ValidationFailed,
            message: "Validation failed.",
            errorCode: "VALIDATION_FAILED",
            errors: Array.AsReadOnly(new[] { error }),
            exception: argumentException,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }

    /// <summary>
    /// Creates a generic failure result from an exception.
    /// </summary>
    private static ServiceResult CreateGenericFailureResult(Exception exception)
    {
        var exceptionTypeName = exception.GetType().Name;
        return new(
            status: ServiceResultStatus.Failure,
            message: exception.Message,
            errorCode: exceptionTypeName,
            errors: Array.AsReadOnly(new[] { ServiceError.Exception(exceptionTypeName, exception.Message) }),
            exception: exception,
            traceId: GetCurrentTraceId(),
            timestampUtc: DateTimeOffset.UtcNow,
            metadata: EmptyMetadata);
    }
}
