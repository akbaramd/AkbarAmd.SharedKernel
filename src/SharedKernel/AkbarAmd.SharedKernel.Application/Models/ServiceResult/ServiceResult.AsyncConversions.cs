// File: ServiceResult.AsyncConversions.cs
#nullable enable
using System.Threading.Tasks;

namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides implicit conversions between service results and asynchronous task types.
/// </summary>
/// <remarks>
/// These conversions enable seamless integration with async/await patterns, allowing service results
/// to be returned directly from async methods without explicit wrapping.
/// </remarks>
public sealed partial class ServiceResult
{
    /// <summary>
    /// Implicitly converts a result to a completed <see cref="Task{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to wrap in a task.</param>
    /// <returns>A completed task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static implicit operator Task<ServiceResult>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Implicitly converts a result to a completed <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to wrap in a value task.</param>
    /// <returns>A completed value task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static implicit operator ValueTask<ServiceResult>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult>(result);
    }

    /// <summary>
    /// Implicitly converts a result to a completed <see cref="Task{TResult}"/> of <see cref="ServiceResult{Unit}"/>.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a task.</param>
    /// <returns>A completed task containing a unit-typed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator Task<ServiceResult<Unit>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult<Unit>.FromNonGeneric(result, Unit.Value));
    }

    /// <summary>
    /// Implicitly converts a result to a completed <see cref="ValueTask{TResult}"/> of <see cref="ServiceResult{Unit}"/>.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a value task.</param>
    /// <returns>A completed value task containing a unit-typed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator ValueTask<ServiceResult<Unit>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<Unit>>(ServiceResult<Unit>.FromNonGeneric(result, Unit.Value));
    }

    /// <summary>
    /// Implicitly converts a result to a completed Task of ServiceResult with object? as the value type.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a task.</param>
    /// <returns>A completed task containing an object-typed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator Task<ServiceResult<object?>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult<object?>.FromNonGeneric(result, value: null));
    }

    /// <summary>
    /// Implicitly converts a result to a completed ValueTask of ServiceResult with object? as the value type.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a value task.</param>
    /// <returns>A completed value task containing an object-typed result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator ValueTask<ServiceResult<object?>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<object?>>(ServiceResult<object?>.FromNonGeneric(result, value: null));
    }

    /// <summary>
    /// Explicitly extracts a result from a task by synchronously waiting for its completion.
    /// </summary>
    /// <param name="task">The task to extract the result from.</param>
    /// <returns>The result from the completed task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the task is not completed.</exception>
    /// <remarks>
    /// This explicit conversion should be used with caution, as it blocks the current thread.
    /// Prefer using <c>await</c> for asynchronous operations.
    /// </remarks>
    public static explicit operator ServiceResult(Task<ServiceResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Explicitly extracts a result from a value task by synchronously waiting for its completion.
    /// </summary>
    /// <param name="task">The value task to extract the result from.</param>
    /// <returns>The result from the completed task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the task is not completed.</exception>
    /// <remarks>
    /// This explicit conversion should be used with caution, as it blocks the current thread.
    /// Prefer using <c>await</c> for asynchronous operations.
    /// </remarks>
    public static explicit operator ServiceResult(ValueTask<ServiceResult> task)
        => task.GetAwaiter().GetResult();
}

/// <summary>
/// Provides implicit conversions between generic service results and asynchronous task types.
/// </summary>
/// <typeparam name="T">The type of the value in the service result.</typeparam>
public sealed partial class ServiceResult<T>
{
    /// <summary>
    /// Implicitly converts a result to a completed <see cref="Task{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to wrap in a task.</param>
    /// <returns>A completed task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static implicit operator Task<ServiceResult<T>>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Implicitly converts a result to a completed <see cref="ValueTask{TResult}"/>.
    /// </summary>
    /// <param name="result">The result to wrap in a value task.</param>
    /// <returns>A completed value task containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static implicit operator ValueTask<ServiceResult<T>>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<T>>(result);
    }

    /// <summary>
    /// Implicitly converts a generic result to a completed <see cref="Task{TResult}"/> of non-generic result.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a task.</param>
    /// <returns>A completed task containing a non-generic result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator Task<ServiceResult>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult.FromBase(result));
    }

    /// <summary>
    /// Implicitly converts a generic result to a completed <see cref="ValueTask{TResult}"/> of non-generic result.
    /// </summary>
    /// <param name="result">The result to convert and wrap in a value task.</param>
    /// <returns>A completed value task containing a non-generic result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This direct conversion avoids a two-step conversion chain, improving performance.
    /// </remarks>
    public static implicit operator ValueTask<ServiceResult>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult>(ServiceResult.FromBase(result));
    }

    /// <summary>
    /// Explicitly extracts a result from a task by synchronously waiting for its completion.
    /// </summary>
    /// <param name="task">The task to extract the result from.</param>
    /// <returns>The result from the completed task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the task is not completed.</exception>
    /// <remarks>
    /// This explicit conversion should be used with caution, as it blocks the current thread.
    /// Prefer using <c>await</c> for asynchronous operations.
    /// </remarks>
    public static explicit operator ServiceResult<T>(Task<ServiceResult<T>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Explicitly extracts a result from a value task by synchronously waiting for its completion.
    /// </summary>
    /// <param name="task">The value task to extract the result from.</param>
    /// <returns>The result from the completed task.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the task is not completed.</exception>
    /// <remarks>
    /// This explicit conversion should be used with caution, as it blocks the current thread.
    /// Prefer using <c>await</c> for asynchronous operations.
    /// </remarks>
    public static explicit operator ServiceResult<T>(ValueTask<ServiceResult<T>> task)
        => task.GetAwaiter().GetResult();
}
