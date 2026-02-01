// File: ServiceResult.Status.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Represents the status of a service operation result.
/// </summary>
/// <remarks>
/// Using <see cref="byte"/> as the underlying type for memory efficiency in high-throughput scenarios.
/// </remarks>
public enum ServiceResultStatus : byte
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The operation failed with a general error.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// The operation failed due to validation errors.
    /// </summary>
    ValidationFailed = 2,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// The operation failed due to unauthorized access.
    /// </summary>
    Unauthorized = 4,

    /// <summary>
    /// The operation failed due to insufficient permissions.
    /// </summary>
    Forbidden = 5,

    /// <summary>
    /// The operation failed due to a conflict with the current state.
    /// </summary>
    Conflict = 6,

    /// <summary>
    /// The service is currently unavailable.
    /// </summary>
    Unavailable = 7,

    /// <summary>
    /// The operation timed out.
    /// </summary>
    Timeout = 8,

    /// <summary>
    /// The operation was cancelled.
    /// </summary>
    Cancelled = 9,
}

/// <summary>
/// Represents the category of a service error for classification and handling purposes.
/// </summary>
/// <remarks>
/// Using <see cref="byte"/> as the underlying type for memory efficiency.
/// </remarks>
public enum ServiceErrorCategory : byte
{
    /// <summary>
    /// A general failure error.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// A validation error indicating invalid input.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// An error indicating a resource was not found.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// An error indicating unauthorized access.
    /// </summary>
    Unauthorized = 3,

    /// <summary>
    /// An error indicating insufficient permissions.
    /// </summary>
    Forbidden = 4,

    /// <summary>
    /// An error indicating a conflict with the current state.
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// An error indicating the service is unavailable.
    /// </summary>
    Unavailable = 6,

    /// <summary>
    /// An error indicating a timeout occurred.
    /// </summary>
    Timeout = 7,

    /// <summary>
    /// An error indicating the operation was cancelled.
    /// </summary>
    Cancelled = 8,

    /// <summary>
    /// An error originating from an exception.
    /// </summary>
    Exception = 9,
}

/// <summary>
/// Represents a detailed error that occurred during a service operation.
/// </summary>
/// <param name="Code">The error code that uniquely identifies the type of error.</param>
/// <param name="Message">The human-readable error message.</param>
/// <param name="Category">The category of the error for classification purposes.</param>
/// <param name="Target">The target field, property, or resource that the error relates to, if applicable.</param>
/// <param name="AttemptedValue">The value that was attempted, if applicable (e.g., for validation errors).</param>
/// <remarks>
/// This record type provides immutable error information with factory methods for common error scenarios.
/// </remarks>
public sealed record ServiceError(
    string Code,
    string Message,
    ServiceErrorCategory Category = ServiceErrorCategory.Failure,
    string? Target = null,
    object? AttemptedValue = null)
{
    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">The validation error code.</param>
    /// <param name="message">The validation error message.</param>
    /// <param name="target">The field or property that failed validation, if applicable.</param>
    /// <param name="attemptedValue">The value that was attempted, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Validation"/>.</returns>
    public static ServiceError Validation(string code, string message, string? target = null, object? attemptedValue = null)
        => new(code, message, ServiceErrorCategory.Validation, target, attemptedValue);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The resource that was not found, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.NotFound"/>.</returns>
    public static ServiceError NotFound(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.NotFound, target);

    /// <summary>
    /// Creates an unauthorized access error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The resource that was accessed, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Unauthorized"/>.</returns>
    public static ServiceError Unauthorized(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Unauthorized, target);

    /// <summary>
    /// Creates a forbidden access error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The resource that was accessed, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Forbidden"/>.</returns>
    public static ServiceError Forbidden(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Forbidden, target);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The resource that caused the conflict, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Conflict"/>.</returns>
    public static ServiceError Conflict(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Conflict, target);

    /// <summary>
    /// Creates an unavailable service error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The service that is unavailable, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Unavailable"/>.</returns>
    public static ServiceError Unavailable(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Unavailable, target);

    /// <summary>
    /// Creates a timeout error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The operation that timed out, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Timeout"/>.</returns>
    public static ServiceError Timeout(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Timeout, target);

    /// <summary>
    /// Creates a cancellation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The operation that was cancelled, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Cancelled"/>.</returns>
    public static ServiceError Cancelled(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Cancelled, target);

    /// <summary>
    /// Creates an exception-based error.
    /// </summary>
    /// <param name="code">The error code, typically the exception type name.</param>
    /// <param name="message">The error message, typically the exception message.</param>
    /// <param name="target">The target that caused the exception, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Exception"/>.</returns>
    public static ServiceError Exception(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Exception, target);

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">The target that caused the failure, if applicable.</param>
    /// <returns>A new <see cref="ServiceError"/> with category <see cref="ServiceErrorCategory.Failure"/>.</returns>
    public static ServiceError Failure(string code, string message, string? target = null)
        => new(code, message, ServiceErrorCategory.Failure, target);
}
