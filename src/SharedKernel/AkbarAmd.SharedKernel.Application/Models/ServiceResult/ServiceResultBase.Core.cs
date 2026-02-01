using System.Collections.ObjectModel;

namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Base class for service operation results, providing a standardized way to represent
/// the outcome of service operations including success, failure, and error information.
/// </summary>
/// <remarks>
/// This class implements an immutable result pattern that encapsulates operation status,
/// messages, errors, and metadata. It provides a foundation for type-safe error handling
/// without exceptions for expected failure scenarios.
/// </remarks>
public abstract partial class ServiceResultBase
{
    /// <summary>
    /// An empty, read-only list of errors used as a default value to avoid allocations.
    /// </summary>
    internal static readonly IReadOnlyList<ServiceError> EmptyErrors = Array.AsReadOnly(Array.Empty<ServiceError>());

    /// <summary>
    /// An empty, read-only dictionary of metadata used as a default value to avoid allocations.
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> EmptyMetadata =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(0, StringComparer.Ordinal));

    /// <summary>
    /// Gets the status of the service operation.
    /// </summary>
    /// <value>The status indicating success, failure, or specific error conditions.</value>
    public ServiceResultStatus Status { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    /// <value><c>true</c> if the status is <see cref="ServiceResultStatus.Success"/>; otherwise, <c>false</c>.</value>
    public bool IsSuccess => Status == ServiceResultStatus.Success;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    /// <value><c>true</c> if the status is not <see cref="ServiceResultStatus.Success"/>; otherwise, <c>false</c>.</value>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the primary message describing the operation result.
    /// </summary>
    /// <value>The message, or <c>null</c> if no message is provided.</value>
    public string? Message { get; }

    /// <summary>
    /// Gets the error code associated with the failure, if applicable.
    /// </summary>
    /// <value>The error code, or <c>null</c> if no error code is provided.</value>
    public string? ErrorCode { get; }

    /// <summary>
    /// Gets the collection of detailed error information.
    /// </summary>
    /// <value>A read-only list of errors, never <c>null</c>.</value>
    public IReadOnlyList<ServiceError> Errors { get; }

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    /// <value>The exception, or <c>null</c> if no exception is associated.</value>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the trace identifier for correlation across distributed systems.
    /// </summary>
    /// <value>The trace ID, or <c>null</c> if no trace ID is available.</value>
    public string? TraceId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the result was created.
    /// </summary>
    /// <value>The timestamp in UTC.</value>
    public DateTimeOffset TimestampUtc { get; }

    /// <summary>
    /// Gets additional metadata associated with the result.
    /// </summary>
    /// <value>A read-only dictionary of key-value pairs, never <c>null</c>.</value>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResultBase"/> class.
    /// </summary>
    /// <param name="status">The status of the operation.</param>
    /// <param name="message">The primary message describing the result.</param>
    /// <param name="errorCode">The error code, if applicable.</param>
    /// <param name="errors">The collection of detailed errors.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <param name="traceId">The trace identifier for correlation.</param>
    /// <param name="timestampUtc">The UTC timestamp when the result was created.</param>
    /// <param name="metadata">Additional metadata associated with the result.</param>
    protected ServiceResultBase(
        ServiceResultStatus status,
        string? message,
        string? errorCode,
        IReadOnlyList<ServiceError> errors,
        Exception? exception,
        string? traceId,
        DateTimeOffset timestampUtc,
        IReadOnlyDictionary<string, string> metadata)
    {
        Status = status;
        Message = message;
        ErrorCode = errorCode;
        Errors = errors ?? EmptyErrors;
        Exception = exception;
        TraceId = traceId;
        TimestampUtc = timestampUtc;
        Metadata = metadata ?? EmptyMetadata;
    }

    /// <summary>
    /// Normalizes a collection of errors into a read-only list, handling null and empty cases efficiently.
    /// </summary>
    /// <param name="errors">The errors to normalize, which may be <c>null</c> or empty.</param>
    /// <returns>A read-only list of errors, or <see cref="EmptyErrors"/> if the input is null or empty.</returns>
    /// <exception cref="ArgumentException">Thrown when the collection contains null elements.</exception>
    internal static IReadOnlyList<ServiceError> NormalizeErrors(IEnumerable<ServiceError>? errors)
    {
        if (errors is null)
            return EmptyErrors;

        // Fast path for empty collections
        if (errors is IReadOnlyCollection<ServiceError> readOnlyCollection && readOnlyCollection.Count == 0)
            return EmptyErrors;

        // Materialize to array for efficient processing
        var array = errors as ServiceError[] ?? errors.ToArray();
        if (array.Length == 0)
            return EmptyErrors;

        // Validate no null elements (using Array.IndexOf for better performance than LINQ)
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] is null)
                throw new ArgumentException("Errors cannot contain null elements.", nameof(errors));
        }

        return Array.AsReadOnly(array);
    }

    /// <summary>
    /// Normalizes metadata into a read-only dictionary with case-insensitive key comparison.
    /// </summary>
    /// <param name="metadata">The metadata to normalize, which may be <c>null</c> or empty.</param>
    /// <returns>
    /// A read-only dictionary with ordinal string comparison, or <see cref="EmptyMetadata"/> if the input is null or empty.
    /// </returns>
    internal static IReadOnlyDictionary<string, string> NormalizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return EmptyMetadata;

        // Reference equality check for performance
        if (ReferenceEquals(metadata, EmptyMetadata))
            return EmptyMetadata;

        // If already a ReadOnlyDictionary, return as-is (assuming it has correct comparer)
        if (metadata is ReadOnlyDictionary<string, string> readOnlyDict)
            return readOnlyDict;

        // Create new dictionary with ordinal comparer for consistent behavior
        return new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata, StringComparer.Ordinal));
    }

    /// <summary>
    /// Returns a string representation of the service result.
    /// </summary>
    /// <returns>A formatted string containing the result type, status, and key information.</returns>
    public override string ToString()
    {
        if (IsSuccess)
        {
            return string.IsNullOrEmpty(Message)
                ? $"{GetType().Name}(Success)"
                : $"{GetType().Name}(Success, Message='{Message}')";
        }

        var errorCode = ErrorCode ?? "N/A";
        var message = Message ?? "N/A";
        return $"{GetType().Name}({Status}, Code='{errorCode}', Message='{message}', Errors={Errors.Count})";
    }
}
    