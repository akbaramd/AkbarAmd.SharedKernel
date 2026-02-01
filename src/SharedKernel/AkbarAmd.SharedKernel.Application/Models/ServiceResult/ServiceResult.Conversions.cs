// File: ServiceResult.Conversions.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Provides implicit conversions between non-generic and generic service results for common scenarios.
/// </summary>
public partial class ServiceResult
{
    /// <summary>
    /// Implicitly converts a non-generic result to a generic result with object? as the value type.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>
    /// A generic result with the same status and error information, with <c>null</c> as the value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This conversion is safe for both success and failure scenarios, as object? can represent
    /// any value type including <c>null</c>.
    /// </remarks>
    public static implicit operator ServiceResult<object?>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return ServiceResult<object?>.FromNonGeneric(result, value: null);
    }

    /// <summary>
    /// Implicitly converts a non-generic result to a generic result with <see cref="Unit"/> as the value type.
    /// </summary>
    /// <param name="result">The non-generic result to convert.</param>
    /// <returns>
    /// A generic result with the same status and error information, with <see cref="Unit.Value"/> as the value.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <remarks>
    /// This conversion is safe for both success and failure scenarios. <see cref="Unit"/> is useful
    /// when an operation succeeds but doesn't return a meaningful value.
    /// </remarks>
    public static implicit operator ServiceResult<Unit>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return ServiceResult<Unit>.FromNonGeneric(result, value: Unit.Value);
    }
}
