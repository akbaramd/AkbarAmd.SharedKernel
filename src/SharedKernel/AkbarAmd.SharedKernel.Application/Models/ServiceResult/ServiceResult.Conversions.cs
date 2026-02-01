// File: ServiceResult.Conversions.cs
#nullable enable
using System;

namespace AkbarAmd.SharedKernel.Application.Results;

public partial class ServiceResult
{
    // Exception -> ServiceResult (so you can: return ex;)
    public static implicit operator ServiceResult(Exception exception) => FromException(exception);

    // Non-generic -> generic(object) (so you can: ServiceResult<object> r = ServiceResult.Ok();)
    public static implicit operator ServiceResult<object>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result is ServiceResult<object> obj)
            return obj;

        var value = result is IUntypedServiceResult u ? u.UntypedValue : null;
        return ServiceResult<object>.FromResult(result, value);
    }
}

public sealed partial class ServiceResult<T>
{
    // Value -> ServiceResult<T> (so you can: return dto;)
    public static implicit operator ServiceResult<T>(T value) => Ok(value);

    // Exception -> ServiceResult<T> (so you can: return ex;)
    public static implicit operator ServiceResult<T>(Exception exception)
        => FromResult(ServiceResult.FromException(exception));

    // Explicit: ServiceResult -> ServiceResult<T> (useful when you know it’s safe)
    public static explicit operator ServiceResult<T>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result is ServiceResult<T> typed)
            return typed;

        if (result is IUntypedServiceResult u)
        {
            if (u.UntypedValue is T casted)
                return FromResult(result, casted);

            if (u.UntypedValue is null)
                return FromResult(result, default);

            throw new InvalidCastException(
                $"Cannot cast service result value of type '{u.UntypedValue.GetType().FullName}' to '{typeof(T).FullName}'.");
        }

        return FromResult(result, default);
    }
}