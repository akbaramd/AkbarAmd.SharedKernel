// File: ServiceResult.AsyncExtensions.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides extension methods for asynchronous operations with service results.
/// </summary>
public static class ServiceResultAsyncExtensions
{
    /// <summary>
    /// Wraps a result in a completed task.
    /// </summary>
    /// <param name="result">The result to wrap.</param>
    /// <returns>A completed task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static Task<ServiceResult> AsTask(this ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Wraps a generic result in a completed task.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="result">The result to wrap.</param>
    /// <returns>A completed task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static Task<ServiceResult<T>> AsTask<T>(this ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Wraps a result in a completed value task.
    /// </summary>
    /// <param name="result">The result to wrap.</param>
    /// <returns>A completed value task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ValueTask<ServiceResult> AsValueTask(this ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult>(result);
    }

    /// <summary>
    /// Wraps a generic result in a completed value task.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="result">The result to wrap.</param>
    /// <returns>A completed value task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ValueTask<ServiceResult<T>> AsValueTask<T>(this ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<T>>(result);
    }

    /// <summary>
    /// Asynchronously transforms the value of a successful result using the specified mapping function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the transformed value.</typeparam>
    /// <param name="task">The task containing the result to transform.</param>
    /// <param name="map">The function to apply to the value if the result is successful.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that completes with a new result containing the transformed value if successful;
    /// otherwise, a failure result with the same error information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> or <paramref name="map"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    /// <remarks>
    /// This method provides an asynchronous version of the <see cref="ServiceResult{T}.Map{TOut}(Func{T, TOut})"/> method.
    /// </remarks>
    public static async Task<ServiceResult<TOut>> MapAsync<TIn, TOut>(
        this Task<ServiceResult<TIn>> task,
        Func<TIn, TOut> map,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(map);

        cancellationToken.ThrowIfCancellationRequested();
        var result = await task.ConfigureAwait(false);
        return result.Map(map);
    }

    /// <summary>
    /// Asynchronously binds a function that returns a service result to a successful result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the value in the resulting service result.</typeparam>
    /// <param name="task">The task containing the result to bind.</param>
    /// <param name="bind">The function to apply to the value if the result is successful.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that completes with the result of applying <paramref name="bind"/> to the value if successful;
    /// otherwise, a failure result with the same error information.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> or <paramref name="bind"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    /// <remarks>
    /// This method provides an asynchronous version of the <see cref="ServiceResult{T}.Bind{TOut}(Func{T, ServiceResult{TOut}})"/> method.
    /// </remarks>
    public static async Task<ServiceResult<TOut>> BindAsync<TIn, TOut>(
        this Task<ServiceResult<TIn>> task,
        Func<TIn, Task<ServiceResult<TOut>>> bind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(bind);

        cancellationToken.ThrowIfCancellationRequested();
        var result = await task.ConfigureAwait(false);
        if (!result.IsSuccess)
            return ServiceResult<TOut>.FromBase(result);

        return await bind(result.Value!).ConfigureAwait(false);
    }
}
