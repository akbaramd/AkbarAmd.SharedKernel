// File: ServiceResult.AsyncConversions.Tests.cs
#nullable enable
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;
using Xunit;

namespace AkbarAmd.SharedKernel.Tests.Application.Results;

public sealed class ServiceResult_AsyncConversions_Tests
{
    [Fact]
    public async Task Implicit_ServiceResultT_to_Task_Wraps()
    {
        ServiceResult<int> r = 10;

        Task<ServiceResult<int>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(10, awaited.Value);
    }

 

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultObject_AllowsSuccess()
    {
        Task<ServiceResult<object?>> t = ServiceResult.Ok("ok"); // success allowed for object?

        var r = await t;
        Assert.True(r.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, r.Status);
        Assert.Equal("ok", r.Message);
        Assert.Null(r.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultUnit_AllowsSuccess()
    {
        Task<ServiceResult<Unit>> t = ServiceResult.Ok("ok"); // success allowed for Unit

        var r = await t;
        Assert.True(r.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, r.Status);
        Assert.Equal("ok", r.Message);
        Assert.Equal(Unit.Value, r.Value);
    }

    [Fact]
    public void Explicit_Task_to_ServiceResultT_Unwraps()
    {
        Task<ServiceResult<int>> t = Task.FromResult(ServiceResult<int>.Ok(5));

        var r = (ServiceResult<int>)t; // explicit (blocking)
        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Explicit_Task_to_ServiceResult_Unwraps()
    {
        Task<ServiceResult> t = Task.FromResult(ServiceResult.Ok("ok"));

        var r = (ServiceResult)t; // explicit (blocking)
        Assert.True(r.IsSuccess);
        Assert.Equal("ok", r.Message);
    }
}
