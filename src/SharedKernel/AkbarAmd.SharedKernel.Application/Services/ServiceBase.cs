// File: ServiceBase.cs
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;
using FluentValidation.Results;

namespace AkbarAmd.SharedKernel.Application.Services;

/// <summary>
/// Base class for service implementations, providing helper methods for creating and handling service results.
/// </summary>
/// <remarks>
/// This abstract class simplifies the creation of <see cref="ServiceResult"/> instances by providing
/// convenient factory methods that automatically apply trace ID correlation. Derived classes should
/// override <see cref="DefaultTraceId"/> to provide custom trace ID handling.
/// </remarks>
public abstract partial class ServiceBase
{
    /// <summary>
    /// Gets the default trace ID to use for service results created by this service.
    /// </summary>
    /// <value>
    /// The default trace ID, or <c>null</c> to use the current activity's trace ID.
    /// Override this property in derived classes to provide custom trace ID handling.
    /// </value>
    protected virtual string? DefaultTraceId => null;

    // ---------- Synchronous Result Helpers ----------

    /// <summary>
    /// Creates a successful service result.
    /// </summary>
    /// <param name="message">An optional success message.</param>
    /// <returns>A successful <see cref="ServiceResult"/> with the specified message and trace ID.</returns>
    protected ServiceResult Ok(string? message = null)
        => ServiceResult.Ok(message).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a successful service result with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The operation result value.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A successful <see cref="ServiceResult{T}"/> with the specified value, message, and trace ID.</returns>
    protected ServiceResult<T> Ok<T>(T value, string? message = null)
        => ServiceResult<T>.Ok(value, message).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a failure service result.
    /// </summary>
    /// <param name="message">The failure message. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code.</param>
    /// <param name="exception">An optional exception that caused the failure.</param>
    /// <returns>A failed <see cref="ServiceResult"/> with the specified error information and trace ID.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    protected ServiceResult Fail(string message, string? code = null, Exception? exception = null)
        => ServiceResult.Fail(message, code, exception).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a failure service result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The failure message. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code.</param>
    /// <param name="exception">An optional exception that caused the failure.</param>
    /// <returns>A failed <see cref="ServiceResult{T}"/> with the specified error information and trace ID.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    protected ServiceResult<T> Fail<T>(string message, string? code = null, Exception? exception = null)
        => ServiceResult<T>.Fail(message, code, exception).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a not found service result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Not found."</param>
    /// <param name="code">An optional error code. Defaults to "NOT_FOUND".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.NotFound"/> and trace ID.</returns>
    protected ServiceResult NotFound(string message = "Not found.", string? code = null, string? target = null)
        => ServiceResult.NotFound(message, code, target).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a not found service result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Not found."</param>
    /// <param name="code">An optional error code. Defaults to "NOT_FOUND".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.NotFound"/> and trace ID.</returns>
    protected ServiceResult<T> NotFound<T>(string message = "Not found.", string? code = null, string? target = null)
        => ServiceResult<T>.FromNonGeneric(ServiceResult.NotFound(message, code, target)).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates an unauthorized access service result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Unauthorized."</param>
    /// <param name="code">An optional error code. Defaults to "UNAUTHORIZED".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Unauthorized"/> and trace ID.</returns>
    protected ServiceResult Unauthorized(string message = "Unauthorized.", string? code = null, string? target = null)
        => ServiceResult.Unauthorized(message, code, target).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates an unauthorized access service result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Unauthorized."</param>
    /// <param name="code">An optional error code. Defaults to "UNAUTHORIZED".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Unauthorized"/> and trace ID.</returns>
    protected ServiceResult<T> Unauthorized<T>(string message = "Unauthorized.", string? code = null, string? target = null)
        => ServiceResult<T>.FromNonGeneric(ServiceResult.Unauthorized(message, code, target)).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a forbidden access service result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Forbidden."</param>
    /// <param name="code">An optional error code. Defaults to "FORBIDDEN".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Forbidden"/> and trace ID.</returns>
    protected ServiceResult Forbidden(string message = "Forbidden.", string? code = null, string? target = null)
        => ServiceResult.Forbidden(message, code, target).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a forbidden access service result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Forbidden."</param>
    /// <param name="code">An optional error code. Defaults to "FORBIDDEN".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Forbidden"/> and trace ID.</returns>
    protected ServiceResult<T> Forbidden<T>(string message = "Forbidden.", string? code = null, string? target = null)
        => ServiceResult<T>.FromNonGeneric(ServiceResult.Forbidden(message, code, target)).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a conflict service result.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Conflict."</param>
    /// <param name="code">An optional error code. Defaults to "CONFLICT".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Conflict"/> and trace ID.</returns>
    protected ServiceResult Conflict(string message = "Conflict.", string? code = null, string? target = null)
        => ServiceResult.Conflict(message, code, target).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a conflict service result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Conflict."</param>
    /// <param name="code">An optional error code. Defaults to "CONFLICT".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Conflict"/> and trace ID.</returns>
    protected ServiceResult<T> Conflict<T>(string message = "Conflict.", string? code = null, string? target = null)
        => ServiceResult<T>.FromNonGeneric(ServiceResult.Conflict(message, code, target)).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a validation failed service result from a FluentValidation result.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <returns>
    /// A <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.ValidationFailed"/> and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    protected ServiceResult Validation(ValidationResult validationResult, string? message = null)
        => ServiceResult.Validation(validationResult, message).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a validation failed service result from a FluentValidation result with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.ValidationFailed"/> and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    protected ServiceResult<T> Validation<T>(ValidationResult validationResult, string? message = null)
        => ServiceResult<T>.Validation(validationResult, message).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a service result from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert to a service result.</param>
    /// <returns>
    /// A <see cref="ServiceResult"/> with an appropriate status based on the exception type and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    protected ServiceResult FromException(Exception exception)
        => ServiceResult.FromException(exception).WithTraceId(DefaultTraceId);

    /// <summary>
    /// Creates a service result from an exception with a typed value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="exception">The exception to convert to a service result.</param>
    /// <returns>
    /// A <see cref="ServiceResult{T}"/> with an appropriate status based on the exception type and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    protected ServiceResult<T> FromException<T>(Exception exception)
        => ServiceResult<T>.FromException(exception).WithTraceId(DefaultTraceId);

    // ---------- Asynchronous Result Helpers ----------

    /// <summary>
    /// Creates a successful service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">An optional success message.</param>
    /// <returns>A completed task containing a successful <see cref="ServiceResult"/>.</returns>
    protected Task<ServiceResult> OkAsync(string? message = null)
        => Ok(message).AsTask();

    /// <summary>
    /// Creates a successful service result with a value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The operation result value.</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A completed task containing a successful <see cref="ServiceResult{T}"/>.</returns>
    protected Task<ServiceResult<T>> OkAsync<T>(T value, string? message = null)
        => Ok(value, message).AsTask();

    /// <summary>
    /// Creates a failure service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">The failure message. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code.</param>
    /// <param name="exception">An optional exception that caused the failure.</param>
    /// <returns>A completed task containing a failed <see cref="ServiceResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    protected Task<ServiceResult> FailAsync(string message, string? code = null, Exception? exception = null)
        => Fail(message, code, exception).AsTask();

    /// <summary>
    /// Creates a failure service result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The failure message. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code.</param>
    /// <param name="exception">An optional exception that caused the failure.</param>
    /// <returns>A completed task containing a failed <see cref="ServiceResult{T}"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    protected Task<ServiceResult<T>> FailAsync<T>(string message, string? code = null, Exception? exception = null)
        => Fail<T>(message, code, exception).AsTask();

    /// <summary>
    /// Creates a not found service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Not found."</param>
    /// <param name="code">An optional error code. Defaults to "NOT_FOUND".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.NotFound"/>.</returns>
    protected Task<ServiceResult> NotFoundAsync(string message = "Not found.", string? code = null, string? target = null)
        => NotFound(message, code, target).AsTask();

    /// <summary>
    /// Creates a not found service result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Not found."</param>
    /// <param name="code">An optional error code. Defaults to "NOT_FOUND".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.NotFound"/>.</returns>
    protected Task<ServiceResult<T>> NotFoundAsync<T>(string message = "Not found.", string? code = null, string? target = null)
        => NotFound<T>(message, code, target).AsTask();

    /// <summary>
    /// Creates an unauthorized access service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Unauthorized."</param>
    /// <param name="code">An optional error code. Defaults to "UNAUTHORIZED".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Unauthorized"/>.</returns>
    protected Task<ServiceResult> UnauthorizedAsync(string message = "Unauthorized.", string? code = null, string? target = null)
        => Unauthorized(message, code, target).AsTask();

    /// <summary>
    /// Creates an unauthorized access service result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Unauthorized."</param>
    /// <param name="code">An optional error code. Defaults to "UNAUTHORIZED".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Unauthorized"/>.</returns>
    protected Task<ServiceResult<T>> UnauthorizedAsync<T>(string message = "Unauthorized.", string? code = null, string? target = null)
        => Unauthorized<T>(message, code, target).AsTask();

    /// <summary>
    /// Creates a forbidden access service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Forbidden."</param>
    /// <param name="code">An optional error code. Defaults to "FORBIDDEN".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Forbidden"/>.</returns>
    protected Task<ServiceResult> ForbiddenAsync(string message = "Forbidden.", string? code = null, string? target = null)
        => Forbidden(message, code, target).AsTask();

    /// <summary>
    /// Creates a forbidden access service result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Forbidden."</param>
    /// <param name="code">An optional error code. Defaults to "FORBIDDEN".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Forbidden"/>.</returns>
    protected Task<ServiceResult<T>> ForbiddenAsync<T>(string message = "Forbidden.", string? code = null, string? target = null)
        => Forbidden<T>(message, code, target).AsTask();

    /// <summary>
    /// Creates a conflict service result wrapped in a completed task.
    /// </summary>
    /// <param name="message">The error message. Defaults to "Conflict."</param>
    /// <param name="code">An optional error code. Defaults to "CONFLICT".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.Conflict"/>.</returns>
    protected Task<ServiceResult> ConflictAsync(string message = "Conflict.", string? code = null, string? target = null)
        => Conflict(message, code, target).AsTask();

    /// <summary>
    /// Creates a conflict service result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="message">The error message. Defaults to "Conflict."</param>
    /// <param name="code">An optional error code. Defaults to "CONFLICT".</param>
    /// <param name="target">An optional target resource identifier.</param>
    /// <returns>A completed task containing a <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.Conflict"/>.</returns>
    protected Task<ServiceResult<T>> ConflictAsync<T>(string message = "Conflict.", string? code = null, string? target = null)
        => Conflict<T>(message, code, target).AsTask();

    /// <summary>
    /// Creates a validation failed service result from a FluentValidation result wrapped in a completed task.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <returns>
    /// A completed task containing a <see cref="ServiceResult"/> with status <see cref="ServiceResultStatus.ValidationFailed"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    protected Task<ServiceResult> ValidationAsync(ValidationResult validationResult, string? message = null)
        => Validation(validationResult, message).AsTask();

    /// <summary>
    /// Creates a validation failed service result from a FluentValidation result with a typed value wrapped in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="validationResult">The FluentValidation result containing validation failures.</param>
    /// <param name="message">An optional custom message. Defaults to "Validation failed." if not provided.</param>
    /// <returns>
    /// A completed task containing a <see cref="ServiceResult{T}"/> with status <see cref="ServiceResultStatus.ValidationFailed"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="validationResult"/> is null.</exception>
    protected Task<ServiceResult<T>> ValidationAsync<T>(ValidationResult validationResult, string? message = null)
        => Validation<T>(validationResult, message).AsTask();

    // ---------- Exception Handling Helpers ----------

    /// <summary>
    /// Executes an asynchronous action and wraps the result in a service result, catching any exceptions.
    /// </summary>
    /// <param name="action">The asynchronous action to execute.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that completes with a successful result if the action completes without exceptions;
    /// otherwise, a failure result containing the exception information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <remarks>
    /// This method provides a safe way to execute operations that don't return a value, automatically
    /// converting any exceptions to service results. Use this for operations where exceptions are
    /// expected and should be handled as part of the normal flow.
    /// </remarks>
    protected async Task<ServiceResult> TryAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            await action(cancellationToken).ConfigureAwait(false);
            return Ok();
        }
        catch (Exception ex)
        {
            return FromException(ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous function and wraps the result in a service result, catching any exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the value returned by the function.</typeparam>
    /// <param name="action">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that completes with a successful result containing the function's return value if it
    /// completes without exceptions; otherwise, a failure result containing the exception information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <remarks>
    /// This method provides a safe way to execute operations that return a value, automatically
    /// converting any exceptions to service results. Use this for operations where exceptions are
    /// expected and should be handled as part of the normal flow.
    /// </remarks>
    protected async Task<ServiceResult<T>> TryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            var value = await action(cancellationToken).ConfigureAwait(false);
            return Ok(value);
        }
        catch (Exception ex)
        {
            return FromException<T>(ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous function that returns a service result, catching any exceptions.
    /// </summary>
    /// <typeparam name="TOut">The type of the value in the service result.</typeparam>
    /// <param name="action">The asynchronous function that returns a service result.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// The service result returned by the function if it completes without exceptions;
    /// otherwise, a failure result containing the exception information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
    /// <remarks>
    /// This method provides a safe way to execute operations that already return service results,
    /// ensuring that any unexpected exceptions are caught and converted to failure results.
    /// </remarks>
    protected async Task<ServiceResult<TOut>> TryAsync<TOut>(
        Func<CancellationToken, Task<ServiceResult<TOut>>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return FromException<TOut>(ex);
        }
    }
}
