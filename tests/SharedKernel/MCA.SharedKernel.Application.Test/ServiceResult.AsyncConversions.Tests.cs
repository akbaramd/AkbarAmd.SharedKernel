// File: ServiceResult.AsyncConversions.Partial.Tests.cs
#nullable enable
using System;
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;
using Xunit;

namespace AkbarAmd.SharedKernel.Tests.Application.Results;

public sealed class ServiceResult_AsyncConversions_Partial_Tests
{
    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResult_Wraps()
    {
        var r = ServiceResult.Ok("ok");

        Task<ServiceResult> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, awaited.Status);
        Assert.Equal("ok", awaited.Message);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_ValueTask_ServiceResult_Wraps()
    {
        var r = ServiceResult.Ok("ok");

        ValueTask<ServiceResult> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal("ok", awaited.Message);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultUnit_Wraps_And_SynthesizesUnitValue()
    {
        var r = ServiceResult.Ok("ok");

        Task<ServiceResult<Unit>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, awaited.Status);
        Assert.Equal("ok", awaited.Message);
        Assert.Equal(Unit.Value, awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_ValueTask_ServiceResultUnit_Wraps_And_SynthesizesUnitValue()
    {
        var r = ServiceResult.Ok("ok");

        ValueTask<ServiceResult<Unit>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(Unit.Value, awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultObject_Wraps_And_SynthesizesNullValue()
    {
        var r = ServiceResult.Ok("ok");

        Task<ServiceResult<object?>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, awaited.Status);
        Assert.Equal("ok", awaited.Message);
        Assert.Null(awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_ValueTask_ServiceResultObject_Wraps_And_SynthesizesNullValue()
    {
        var r = ServiceResult.Ok("ok");

        ValueTask<ServiceResult<object?>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Null(awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResultT_to_Task_ServiceResultT_Wraps()
    {
        ServiceResult<int> r = 42;

        Task<ServiceResult<int>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(42, awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResultT_to_ValueTask_ServiceResultT_Wraps()
    {
        ServiceResult<int> r = 42;

        ValueTask<ServiceResult<int>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(42, awaited.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResultT_to_Task_ServiceResult_DropsValue_ButPreservesStatus()
    {
        var typed = ServiceResult<int>.Ok(7, "ok");

        Task<ServiceResult> t = typed; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, awaited.Status);
        Assert.Equal("ok", awaited.Message);
        Assert.Null(awaited.Exception);
    }

    [Fact]
    public async Task Implicit_ServiceResultT_to_ValueTask_ServiceResult_DropsValue_ButPreservesStatus()
    {
        var typed = ServiceResult<int>.Ok(7, "ok");

        ValueTask<ServiceResult> t = typed; // implicit

        var awaited = await t;
        Assert.True(awaited.IsSuccess);
        Assert.Equal("ok", awaited.Message);
    }

    [Fact]
    public void Explicit_Task_ServiceResult_to_ServiceResult_Unwraps_Blocking()
    {
        Task<ServiceResult> t = Task.FromResult(ServiceResult.Ok("ok"));

        var r = (ServiceResult)t; // explicit (blocking)

        Assert.True(r.IsSuccess);
        Assert.Equal("ok", r.Message);
    }

    [Fact]
    public void Explicit_ValueTask_ServiceResult_to_ServiceResult_Unwraps_Blocking()
    {
        ValueTask<ServiceResult> t = new(ServiceResult.Ok("ok"));

        var r = (ServiceResult)t; // explicit (blocking)

        Assert.True(r.IsSuccess);
        Assert.Equal("ok", r.Message);
    }

    [Fact]
    public void Explicit_Task_ServiceResultT_to_ServiceResultT_Unwraps_Blocking()
    {
        Task<ServiceResult<int>> t = Task.FromResult(ServiceResult<int>.Ok(5));

        var r = (ServiceResult<int>)t; // explicit (blocking)

        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public void Explicit_ValueTask_ServiceResultT_to_ServiceResultT_Unwraps_Blocking()
    {
        ValueTask<ServiceResult<int>> t = new(ServiceResult<int>.Ok(5));

        var r = (ServiceResult<int>)t; // explicit (blocking)

        Assert.True(r.IsSuccess);
        Assert.Equal(5, r.Value);
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultUnit_PreservesFailure()
    {
        var r = ServiceResult.NotFound("missing");

        Task<ServiceResult<Unit>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, awaited.Status);
        Assert.Equal("missing", awaited.Message);
        Assert.Equal(Unit.Value, awaited.Value); // synthesized still
    }

    [Fact]
    public async Task Implicit_ServiceResult_to_Task_ServiceResultObject_PreservesFailure()
    {
        var r = ServiceResult.NotFound("missing");

        Task<ServiceResult<object?>> t = r; // implicit

        var awaited = await t;
        Assert.True(awaited.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, awaited.Status);
        Assert.Equal("missing", awaited.Message);
        Assert.Null(awaited.Value); // synthesized still
    }
}
