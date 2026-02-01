// File: ServiceBase.Partials.Transforms.cs
#nullable enable
using System;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;

namespace AkbarAmd.SharedKernel.Application.Services;

/// <summary>
/// Provides functional transformation methods for service results.
/// </summary>
public abstract partial class ServiceBase
{
    /// <summary>
    /// Transforms the value of a successful service result using the specified mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the transformed value.</typeparam>
    /// <param name="result">The service result to transform.</param>
    /// <param name="map">The function to apply to the value if the result is successful.</param>
    /// <param name="successMessage">An optional success message to use for the transformed result. If not provided, the original message is preserved.</param>
    /// <returns>
    /// A new service result with the transformed value if the input result is successful;
    /// otherwise, a failure result with the same error information and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="map"/> is null.</exception>
    /// <remarks>
    /// This method implements the functor pattern, allowing transformation of values within
    /// the result context without unwrapping. If the result is a failure, the mapping function
    /// is not called and the error information is preserved with the trace ID.
    /// </remarks>
    protected ServiceResult<TOut> Map<TIn, TOut>(
        ServiceResult<TIn> result,
        Func<TIn, TOut> map,
        string? successMessage = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(map);

        if (!result.IsSuccess)
            return ServiceResult<TOut>.FromBase(result).WithTraceId(DefaultTraceId);

        return Ok(map(result.Value!), successMessage ?? result.Message);
    }

    /// <summary>
    /// Binds a function that returns a service result to a successful service result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the value in the resulting service result.</typeparam>
    /// <param name="result">The service result to bind.</param>
    /// <param name="bind">The function to apply to the value if the result is successful.</param>
    /// <returns>
    /// The result of applying <paramref name="bind"/> to the value if successful;
    /// otherwise, a failure result with the same error information and trace ID.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="bind"/> is null.</exception>
    /// <remarks>
    /// This method implements the monadic bind operation, allowing chaining of operations that
    /// return service results. If the result is a failure, the bind function is not called
    /// and the error information is preserved with the trace ID.
    /// </remarks>
    protected ServiceResult<TOut> Bind<TIn, TOut>(
        ServiceResult<TIn> result,
        Func<TIn, ServiceResult<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(bind);

        if (!result.IsSuccess)
            return ServiceResult<TOut>.FromBase(result).WithTraceId(DefaultTraceId);

        return bind(result.Value!).WithTraceId(DefaultTraceId);
    }

    /// <summary>
    /// Ensures a condition is met, returning a success or failure result accordingly.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The failure message to use if the condition is false. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code to use if the condition is false.</param>
    /// <param name="target">An optional target identifier to use if the condition is false.</param>
    /// <returns>
    /// A successful result if <paramref name="condition"/> is <c>true</c>;
    /// otherwise, a failure result with the specified error information and trace ID.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    /// <remarks>
    /// This method provides a convenient way to validate conditions and return appropriate
    /// service results. It's useful for precondition checks and validation logic.
    /// </remarks>
    protected ServiceResult Ensure(
        bool condition,
        string message,
        string? code = null,
        string? target = null)
    {
        if (condition)
            return Ok();

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return ServiceResult.Fail(message, code).WithTraceId(DefaultTraceId);
    }

    /// <summary>
    /// Ensures a condition is met, returning a success or failure result with a typed value accordingly.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The failure message to use if the condition is false. Must not be null or whitespace.</param>
    /// <param name="code">An optional error code to use if the condition is false.</param>
    /// <param name="target">An optional target identifier to use if the condition is false.</param>
    /// <returns>
    /// A successful result with <c>default(T)</c> if <paramref name="condition"/> is <c>true</c>;
    /// otherwise, a failure result with the specified error information and trace ID.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="message"/> is null or whitespace.</exception>
    /// <remarks>
    /// This method provides a convenient way to validate conditions and return appropriate
    /// typed service results. It's useful for precondition checks and validation logic when
    /// a typed result is required.
    /// </remarks>
    protected ServiceResult<T> Ensure<T>(
        bool condition,
        string message,
        string? code = null,
        string? target = null)
    {
        if (condition)
            return Ok<T>(default!, message: null);

        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return ServiceResult<T>.FromNonGeneric(ServiceResult.Fail(message, code)).WithTraceId(DefaultTraceId);
    }
}
