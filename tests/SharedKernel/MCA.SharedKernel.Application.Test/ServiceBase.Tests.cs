// File: ServiceBase.Tests.cs
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;
using Xunit;

namespace AkbarAmd.SharedKernel.Tests.Application.Results;

public sealed class ServiceBase_Tests
{
    private sealed class TestService : ServiceBase
    {
        public TestService(string? traceId) => TraceIdForTest = traceId;

        private string? TraceIdForTest { get; }

        protected override string? DefaultTraceId => TraceIdForTest;

        public ServiceResult CallOk(string? message = null) => Ok(message);
        public ServiceResult<int> CallOkT(int value, string? message = null) => Ok(value, message);

        public ServiceResult CallFail(string message, string? code = null, Exception? ex = null) => Fail(message, code, ex);
        public ServiceResult<int> CallFailT(string message, string? code = null, Exception? ex = null) => Fail<int>(message, code, ex);

        public Task<ServiceResult<int>> CallTryAsyncSuccess(CancellationToken ct = default)
            => TryAsync<int>(async _ =>
            {
                await Task.Yield();
                return 123;
            }, ct);

        public Task<ServiceResult<int>> CallTryAsyncThrows(CancellationToken ct = default)
            => TryAsync<int>(async _ =>
            {
                await Task.Yield();
                throw new InvalidOperationException("boom");
            }, ct);

        public Task<ServiceResult> CallTryAsyncVoidThrows(CancellationToken ct = default)
            => TryAsync(async _ =>
            {
                await Task.Yield();
                throw new TimeoutException("timeout");
            }, ct);

        public ServiceResult<string> CallMap(ServiceResult<int> input)
            => Map(input, x => x.ToString());

        public ServiceResult<int> CallBind(ServiceResult<int> input)
            => Bind(input, x => x > 0 ? Ok(x + 1) : (ServiceResult<int>)ServiceResult.Fail("bad"));
    }

    [Fact]
    public void Ok_Sets_DefaultTraceId()
    {
        var svc = new TestService("trace-x");

        var r = svc.CallOk("ok");

        Assert.True(r.IsSuccess);
        Assert.Equal("trace-x", r.TraceId);
        Assert.Equal("ok", r.Message);
    }

    [Fact]
    public void OkT_Sets_DefaultTraceId_AndValue()
    {
        var svc = new TestService("trace-y");

        var r = svc.CallOkT(7, "ok");

        Assert.True(r.IsSuccess);
        Assert.Equal("trace-y", r.TraceId);
        Assert.Equal(7, r.Value);
        Assert.Equal("ok", r.Message);
    }

    [Fact]
    public void Fail_Sets_DefaultTraceId()
    {
        var svc = new TestService("trace-f");

        var r = svc.CallFail("fail", code: "X");

        Assert.True(r.IsFailure);
        Assert.Equal("trace-f", r.TraceId);
        Assert.Equal("X", r.ErrorCode);
    }

    [Fact]
    public async Task TryAsyncT_Success_ReturnsOk_WithTraceId()
    {
        var svc = new TestService("trace-try-ok");

        var r = await svc.CallTryAsyncSuccess();

        Assert.True(r.IsSuccess);
        Assert.Equal("trace-try-ok", r.TraceId);
        Assert.Equal(123, r.Value);
    }

    [Fact]
    public async Task TryAsyncT_WhenThrows_ConvertsException_ToResult_WithTraceId()
    {
        var svc = new TestService("trace-try-ex");

        var r = await svc.CallTryAsyncThrows();

        Assert.True(r.IsFailure);
        Assert.Equal("trace-try-ex", r.TraceId);
        Assert.NotNull(r.Exception);
        Assert.Contains("boom", r.Message ?? string.Empty);
    }

    [Fact]
    public async Task TryAsyncVoid_WhenThrows_ConvertsException_ToResult_WithTraceId()
    {
        var svc = new TestService("trace-try-timeout");

        var r = await svc.CallTryAsyncVoidThrows();

        Assert.True(r.IsFailure);
        Assert.Equal("trace-try-timeout", r.TraceId);
        Assert.Equal(ServiceResultStatus.Timeout, r.Status);
    }

    [Fact]
    public void Map_Success_MapsValue_AndPreservesTraceId()
    {
        var svc = new TestService("trace-map");
        var input = ServiceResult<int>.Ok(12, "ok").WithTraceId("trace-map");

        var r = svc.CallMap(input);

        Assert.True(r.IsSuccess);
        Assert.Equal("trace-map", r.TraceId); // DefaultTraceId is applied on Ok()
        Assert.Equal("12", r.Value);
    }

    [Fact]
    public void Map_Failure_PreservesFailure_AndTraceId()
    {
        var svc = new TestService("trace-map-f");
        ServiceResult<int> input = (ServiceResult<int>)ServiceResult.NotFound("missing").WithTraceId("input-trace");

        var r = svc.CallMap(input);

        Assert.True(r.IsFailure);
        Assert.Equal("trace-map-f", r.TraceId); // wrapper applies DefaultTraceId
        Assert.Equal(ServiceResultStatus.NotFound, r.Status);
    }

    [Fact]
    public void Bind_Success_BindsNextResult_AndTraceId()
    {
        var svc = new TestService("trace-bind");
        var input = ServiceResult<int>.Ok(5).WithTraceId("input-trace");

        var r = svc.CallBind(input);

        Assert.True(r.IsSuccess);
        Assert.Equal("trace-bind", r.TraceId);
        Assert.Equal(6, r.Value);
    }

    [Fact]
    public void Bind_WhenBindReturnsFailure_PropagatesFailure_AndTraceId()
    {
        var svc = new TestService("trace-bind-f");
        var input = ServiceResult<int>.Ok(-1).WithTraceId("input-trace");

        var r = svc.CallBind(input);

        Assert.True(r.IsFailure);
        Assert.Equal("trace-bind-f", r.TraceId);
        Assert.Equal(ServiceResultStatus.Failure, r.Status);
    }
}
