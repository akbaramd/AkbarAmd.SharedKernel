// File: ServiceResult.Conversions.Tests.cs
#nullable enable
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;

namespace MCA.SharedKernel.Application.Test;

public sealed class ServiceResult_Conversions_Tests
{
    [Fact]
    public void Implicit_Exception_to_ServiceResult_MapsFailure()
    {
        Exception ex = new InvalidOperationException("boom");

        ServiceResult result = ex; // implicit

        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Failure, result.Status);
        Assert.Equal("boom", result.Message);
        Assert.Same(ex, result.Exception);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Implicit_Exception_to_ServiceResultT_MapsFailure()
    {
        Exception ex = new TimeoutException("timeout");

        ServiceResult<int> result = ex; // implicit

        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Timeout, result.Status);
        Assert.Equal("timeout", result.Message);
        Assert.Same(ex, result.Exception);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Implicit_Value_to_ServiceResultT_IsSuccess_AndCarriesValue()
    {
        ServiceResult<int> result = 42; // implicit

        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, result.Status);
        Assert.Equal(42, result.Value);
    }

   

   

    [Fact]
    public void Implicit_ServiceResult_to_ServiceResultT_Failure_AllowsReturnStyle()
    {
        var nonGeneric = ServiceResult.NotFound("x not found");

        ServiceResult<int> typed = nonGeneric; // implicit (allowed only for non-success)

        Assert.True(typed.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, typed.Status);
        Assert.Equal("x not found", typed.Message);
        Assert.Equal(default, typed.Value);
    }

    [Fact]
    public void Implicit_ServiceResult_to_ServiceResultT_Success_Throws()
    {
        var nonGenericSuccess = ServiceResult.Ok("ok");

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            ServiceResult<int> _ = nonGenericSuccess; // implicit should throw
        });

        Assert.Contains("Cannot implicitly convert a successful", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Implicit_ServiceResultT_to_ServiceResult_DropsValue_PreservesStatus()
    {
        ServiceResult<int> typed = ServiceResult<int>.Ok(7, "ok");

        ServiceResult nonGeneric = typed; // implicit

        Assert.True(nonGeneric.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, nonGeneric.Status);
        Assert.Equal("ok", nonGeneric.Message);
        Assert.Null(nonGeneric.Exception);
    }

    [Fact]
    public void Explicit_T_From_ServiceResultT_Success_ReturnsValue()
    {
        ServiceResult<int> typed = 123;

        var value = (int)typed; // explicit

        Assert.Equal(123, value);
    }

    [Fact]
    public void Explicit_T_From_ServiceResultT_Failure_ThrowsWithExceptionOrInvalidOperation()
    {
        var failure = ServiceResult<int>.Fail("fail");

        Assert.ThrowsAny<Exception>(() =>
        {
            var _ = (int)failure; // explicit should throw
        });
    }
}
