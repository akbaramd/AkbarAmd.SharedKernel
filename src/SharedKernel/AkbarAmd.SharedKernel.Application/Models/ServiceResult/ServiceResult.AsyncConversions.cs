// File: ServiceResult.AsyncConversions.Partial.cs  (REPLACE ENTIRE FILE)
#nullable enable
using System;
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;

namespace AkbarAmd.SharedKernel.Application.Results;

public sealed partial class ServiceResult
{
    // ServiceResult -> Task<ServiceResult>
    public static implicit operator Task<ServiceResult>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    public static implicit operator ValueTask<ServiceResult>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult>(result);
    }

    // IMPORTANT: Prevent 2 user-defined conversions chain:
    // ServiceResult -> ServiceResult<Unit> -> Task<ServiceResult<Unit>> (NOT allowed by C#)
    // So we provide direct conversions.
    public static implicit operator Task<ServiceResult<Unit>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult<Unit>.FromNonGeneric(result, Unit.Value));
    }

    public static implicit operator ValueTask<ServiceResult<Unit>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<Unit>>(result);
    }

    public static implicit operator Task<ServiceResult<object?>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult<object?>.FromNonGeneric(result, value: null));
    }

    public static implicit operator ValueTask<ServiceResult<object?>>(ServiceResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<object?>>(ServiceResult<object?>.FromNonGeneric(result, value: null));
    }

    // Task/ValueTask -> ServiceResult (blocking explicit)
    public static explicit operator ServiceResult(Task<ServiceResult> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.GetAwaiter().GetResult();
    }

    public static explicit operator ServiceResult(ValueTask<ServiceResult> task)
        => task.GetAwaiter().GetResult();
}

public sealed partial class ServiceResult<T>
{
    // ServiceResult<T> -> Task<ServiceResult<T>>
    public static implicit operator Task<ServiceResult<T>>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(result);
    }

    public static implicit operator ValueTask<ServiceResult<T>>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult<T>>(result);
    }

    // IMPORTANT: Prevent 2 user-defined conversions chain:
    // ServiceResult<T> -> ServiceResult -> Task<ServiceResult> (NOT allowed by C#)
    // Provide direct conversions.
    public static implicit operator Task<ServiceResult>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return Task.FromResult(ServiceResult.FromBase(result));
    }

    public static implicit operator ValueTask<ServiceResult>(ServiceResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ValueTask<ServiceResult>(ServiceResult.FromBase(result));
    }

    // Task/ValueTask -> ServiceResult<T> (blocking explicit)
    public static explicit operator ServiceResult<T>(Task<ServiceResult<T>> task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.GetAwaiter().GetResult();
    }

    public static explicit operator ServiceResult<T>(ValueTask<ServiceResult<T>> task)
        => task.GetAwaiter().GetResult();
}
