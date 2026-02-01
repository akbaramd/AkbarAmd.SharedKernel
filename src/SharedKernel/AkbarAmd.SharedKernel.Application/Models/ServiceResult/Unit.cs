// File: Unit.cs
#nullable enable
namespace AkbarAmd.SharedKernel.Application.Models.ServiceResult;

/// <summary>
/// Represents a unit type, used to indicate that an operation completes successfully
/// but does not return a meaningful value.
/// </summary>
/// <remarks>
/// This type is useful in functional programming scenarios where you need a type parameter
/// but don't have a meaningful value to return. It's similar to <c>void</c> but can be used
/// as a generic type parameter, enabling consistent handling of operations that don't
/// return values.
/// </remarks>
public readonly struct Unit
{
    /// <summary>
    /// Gets the single instance of <see cref="Unit"/>.
    /// </summary>
    /// <value>The default value of <see cref="Unit"/>, which is the only valid instance.</value>
    public static readonly Unit Value = default;

    /// <summary>
    /// Returns a string representation of the unit value.
    /// </summary>
    /// <returns>The string "()", representing the unit value in functional programming notation.</returns>
    public override string ToString() => "()";
}
