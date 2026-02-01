// File: ServiceResult.Extensions.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides extension methods for <see cref="ServiceResult"/> operations.
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Converts a non-generic <see cref="ServiceResult"/> to a generic <see cref="ServiceResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type parameter for the generic result.</typeparam>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>
    /// A generic result with the same status and error information, with <c>default(T)</c> as the value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static ServiceResult<T> As<T>(this ServiceResult result)
        => ServiceResult<T>.FromNonGeneric(result);

    /// <summary>
    /// Ensures that the result is successful, throwing an exception if it is not.
    /// </summary>
    /// <param name="result">The result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is not successful. The exception from the result is thrown if available.
    /// </exception>
    /// <remarks>
    /// This method is useful for scenarios where a failure is unexpected and should be treated as
    /// an exceptional condition. For expected failures, prefer pattern matching or conditional checks.
    /// </remarks>
    public static void EnsureSuccess(this ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
            return;

        throw result.Exception ?? new InvalidOperationException(result.Message ?? "Operation failed.");
    }
}
