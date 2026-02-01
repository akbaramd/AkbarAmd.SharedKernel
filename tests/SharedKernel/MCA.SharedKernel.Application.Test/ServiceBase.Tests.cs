// File: ServiceBase.Tests.cs
#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using AkbarAmd.SharedKernel.Application.Models.ServiceResult;
using AkbarAmd.SharedKernel.Application.Services;
using FluentValidation.Results;
using Xunit;

namespace MCA.SharedKernel.Application.Test;

/// <summary>
/// Unit tests for <see cref="ServiceBase"/> class functionality.
/// </summary>
/// <remarks>
/// These tests verify that ServiceBase correctly applies trace IDs, creates service results,
/// handles exceptions, and provides functional transformation methods.
/// </remarks>
public sealed class ServiceBaseTests
{
    /// <summary>
    /// Test service implementation for testing ServiceBase functionality.
    /// </summary>
    private sealed class TestService : ServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestService"/> class.
        /// </summary>
        /// <param name="traceId">The trace ID to use for testing.</param>
        public TestService(string? traceId) => TraceIdForTest = traceId;

        /// <summary>
        /// Gets the trace ID for testing purposes.
        /// </summary>
        private string? TraceIdForTest { get; }

        /// <summary>
        /// Gets the default trace ID to use for service results.
        /// </summary>
        protected override string? DefaultTraceId => TraceIdForTest;

        // Result creation methods
        public ServiceResult CallOk(string? message = null) => Ok(message);
        public ServiceResult<int> CallOkT(int value, string? message = null) => Ok(value, message);
        public ServiceResult<int?> CallOkTNullable(int? value, string? message = null) => Ok(value, message);
        public ServiceResult CallFail(string message, string? code = null, Exception? ex = null) => Fail(message, code, ex);
        public ServiceResult<int> CallFailT(string message, string? code = null, Exception? ex = null) => Fail<int>(message, code, ex);
        public ServiceResult CallNotFound(string message = "Not found.", string? code = null, string? target = null) => NotFound(message, code, target);
        public ServiceResult<int> CallNotFoundT(string message = "Not found.", string? code = null, string? target = null) => NotFound<int>(message, code, target);
        public ServiceResult CallUnauthorized(string message = "Unauthorized.", string? code = null, string? target = null) => Unauthorized(message, code, target);
        public ServiceResult CallForbidden(string message = "Forbidden.", string? code = null, string? target = null) => Forbidden(message, code, target);
        public ServiceResult CallConflict(string message = "Conflict.", string? code = null, string? target = null) => Conflict(message, code, target);
        public ServiceResult CallValidation(ValidationResult validationResult, string? message = null) => Validation(validationResult, message);
        public ServiceResult CallFromException(Exception exception) => FromException(exception);

        // Async result creation methods
        public Task<ServiceResult> CallOkAsync(string? message = null) => OkAsync(message);
        public Task<ServiceResult<int>> CallOkAsyncT(int value, string? message = null) => OkAsync(value, message);
        public Task<ServiceResult> CallFailAsync(string message, string? code = null, Exception? ex = null) => FailAsync(message, code, ex);

        // Exception handling methods
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

        public Task<ServiceResult<int>> CallTryAsyncWithServiceResult(CancellationToken ct = default)
            => TryAsync<int>(async _ =>
            {
                await Task.Yield();
                return NotFound<int>("Not found");
            }, ct);

        public Task<ServiceResult<int>> CallTryAsyncCancelled(CancellationToken ct = default)
            => TryAsync<int>(async cancellationToken =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                return 123;
            }, ct);

        // Functional transformation methods
        public ServiceResult<string> CallMap(ServiceResult<int> input, string? successMessage = null)
            => Map(input, x => x.ToString(), successMessage);

        public ServiceResult<int> CallBind(ServiceResult<int> input)
            => Bind(input, x => x > 0 ? Ok(x + 1) : (ServiceResult<int>)ServiceResult.Fail("bad"));

        // Validation helper methods
        public ServiceResult CallEnsure(bool condition, string message, string? code = null, string? target = null)
            => Ensure(condition, message, code, target);

        public ServiceResult<int> CallEnsureT(bool condition, string message, string? code = null, string? target = null)
            => Ensure<int>(condition, message, code, target);
    }

    #region Success Result Creation Tests

    [Fact]
    public void Ok_WhenCalled_ReturnsSuccessResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-x");

        // Act
        var result = service.CallOk("ok");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ServiceResultStatus.Success, result.Status);
        Assert.Equal("trace-x", result.TraceId);
        Assert.Equal("ok", result.Message);
    }

    [Fact]
    public void Ok_WhenCalledWithoutMessage_ReturnsSuccessResult_WithNullMessage()
    {
        // Arrange
        var service = new TestService("trace-y");

        // Act
        var result = service.CallOk();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Message);
        Assert.Equal("trace-y", result.TraceId);
    }

    [Fact]
    public void OkT_WhenCalled_ReturnsSuccessResult_WithValueAndDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-y");
        const int expectedValue = 7;

        // Act
        var result = service.CallOkT(expectedValue, "ok");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-y", result.TraceId);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal("ok", result.Message);
    }

    [Fact]
    public void OkT_WhenCalledWithNullValue_ReturnsSuccessResult_WithNullValue()
    {
        // Arrange
        var service = new TestService("trace-null");

        // Act
        var result = service.CallOkTNullable(null, "null value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("null value", result.Message);
    }

    #endregion

    #region Failure Result Creation Tests

    [Fact]
    public void Fail_WhenCalled_ReturnsFailureResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-f");

        // Act
        var result = service.CallFail("fail", code: "X");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Failure, result.Status);
        Assert.Equal("trace-f", result.TraceId);
        Assert.Equal("X", result.ErrorCode);
        Assert.Equal("fail", result.Message);
    }

    [Fact]
    public void Fail_WhenCalledWithException_ReturnsFailureResult_WithException()
    {
        // Arrange
        var service = new TestService("trace-ex");
        var exception = new InvalidOperationException("test exception");

        // Act
        var result = service.CallFail("fail", ex: exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(exception, result.Exception);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void FailT_WhenCalled_ReturnsTypedFailureResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-ft");

        // Act
        var result = service.CallFailT("fail", code: "Y");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-ft", result.TraceId);
        Assert.Equal("Y", result.ErrorCode);
        Assert.Equal(default(int), result.Value);
    }

    #endregion

    #region Specific Error Type Tests

    [Fact]
    public void NotFound_WhenCalled_ReturnsNotFoundResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-notfound");

        // Act
        var result = service.CallNotFound("Resource not found", target: "userId");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
        Assert.Equal("trace-notfound", result.TraceId);
        Assert.Equal("NOT_FOUND", result.ErrorCode);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void NotFoundT_WhenCalled_ReturnsTypedNotFoundResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-notfound-t");

        // Act
        var result = service.CallNotFoundT("User not found");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
        Assert.Equal("trace-notfound-t", result.TraceId);
    }

    [Fact]
    public void Unauthorized_WhenCalled_ReturnsUnauthorizedResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-unauth");

        // Act
        var result = service.CallUnauthorized("Access denied");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
        Assert.Equal("trace-unauth", result.TraceId);
        Assert.Equal("UNAUTHORIZED", result.ErrorCode);
    }

    [Fact]
    public void Forbidden_WhenCalled_ReturnsForbiddenResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-forbidden");

        // Act
        var result = service.CallForbidden("Insufficient permissions");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Forbidden, result.Status);
        Assert.Equal("trace-forbidden", result.TraceId);
        Assert.Equal("FORBIDDEN", result.ErrorCode);
    }

    [Fact]
    public void Conflict_WhenCalled_ReturnsConflictResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-conflict");

        // Act
        var result = service.CallConflict("Email already exists", target: "email");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Conflict, result.Status);
        Assert.Equal("trace-conflict", result.TraceId);
        Assert.Equal("CONFLICT", result.ErrorCode);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Validation_WhenCalledWithInvalidResult_ReturnsValidationFailedResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-validation");
        var validationResult = new ValidationResult(new[]
        {
            new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
            new FluentValidation.Results.ValidationFailure("Name", "Name is required")
        });

        // Act
        var result = service.CallValidation(validationResult, "Validation failed");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Equal("trace-validation", result.TraceId);
        Assert.Equal(2, result.Errors.Count);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public void FromException_WhenCalledWithKeyNotFoundException_ReturnsNotFoundResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-ex-notfound");
        var exception = new KeyNotFoundException("Key not found");

        // Act
        var result = service.CallFromException(exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
        Assert.Equal("trace-ex-notfound", result.TraceId);
        // Note: NotFound() method doesn't preserve the exception, only the message is used
        Assert.Null(result.Exception);
        Assert.Equal("Key not found", result.Message);
    }

    [Fact]
    public void FromException_WhenCalledWithUnauthorizedAccessException_ReturnsUnauthorizedResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-ex-unauth");
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var result = service.CallFromException(exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Unauthorized, result.Status);
        Assert.Equal("trace-ex-unauth", result.TraceId);
    }

    [Fact]
    public void FromException_WhenCalledWithArgumentException_ReturnsValidationFailedResult_WithDefaultTraceId()
    {
        // Arrange
        var service = new TestService("trace-ex-validation");
        var exception = new ArgumentException("Invalid argument", "paramName");

        // Act
        var result = service.CallFromException(exception);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.ValidationFailed, result.Status);
        Assert.Equal("trace-ex-validation", result.TraceId);
    }

    #endregion

    #region Async Result Creation Tests

    [Fact]
    public async Task OkAsync_WhenCalled_ReturnsCompletedTask_WithSuccessResult()
    {
        // Arrange
        var service = new TestService("trace-async-ok");

        // Act
        var result = await service.CallOkAsync("async ok");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-async-ok", result.TraceId);
        Assert.Equal("async ok", result.Message);
    }

    [Fact]
    public async Task OkAsyncT_WhenCalled_ReturnsCompletedTask_WithSuccessResultAndValue()
    {
        // Arrange
        var service = new TestService("trace-async-okt");
        const int expectedValue = 42;

        // Act
        var result = await service.CallOkAsyncT(expectedValue, "async value");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
        Assert.Equal("trace-async-okt", result.TraceId);
    }

    [Fact]
    public async Task FailAsync_WhenCalled_ReturnsCompletedTask_WithFailureResult()
    {
        // Arrange
        var service = new TestService("trace-async-fail");

        // Act
        var result = await service.CallFailAsync("async fail", code: "ASYNC_FAIL");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-async-fail", result.TraceId);
        Assert.Equal("ASYNC_FAIL", result.ErrorCode);
    }

    #endregion

    #region TryAsync Exception Handling Tests

    [Fact]
    public async Task TryAsyncT_WhenActionSucceeds_ReturnsSuccessResult_WithValueAndTraceId()
    {
        // Arrange
        var service = new TestService("trace-try-ok");

        // Act
        var result = await service.CallTryAsyncSuccess();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-try-ok", result.TraceId);
        Assert.Equal(123, result.Value);
    }

    [Fact]
    public async Task TryAsyncT_WhenActionThrows_ConvertsException_ToFailureResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-try-ex");

        // Act
        var result = await service.CallTryAsyncThrows();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-try-ex", result.TraceId);
        Assert.NotNull(result.Exception);
        Assert.IsType<InvalidOperationException>(result.Exception);
        Assert.Contains("boom", result.Message ?? string.Empty);
    }

    [Fact]
    public async Task TryAsyncVoid_WhenActionThrowsTimeoutException_ConvertsToTimeoutResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-try-timeout");

        // Act
        var result = await service.CallTryAsyncVoidThrows();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-try-timeout", result.TraceId);
        Assert.Equal(ServiceResultStatus.Timeout, result.Status);
        Assert.NotNull(result.Exception);
        Assert.IsType<TimeoutException>(result.Exception);
    }

    [Fact]
    public async Task TryAsyncT_WhenActionReturnsServiceResult_ReturnsThatResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-try-result");

        // Act
        var result = await service.CallTryAsyncWithServiceResult();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
        Assert.Equal("trace-try-result", result.TraceId);
    }

    [Fact]
    public async Task TryAsyncT_WhenCancelled_ReturnsCancelledResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-cancel");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await service.CallTryAsyncCancelled(cts.Token);

        // Assert
        // Note: TryAsync catches all exceptions including OperationCanceledException
        // and converts them to results, so cancellation is converted to a Cancelled result
        Assert.True(result.IsFailure);
        Assert.Equal(ServiceResultStatus.Cancelled, result.Status);
        Assert.Equal("trace-cancel", result.TraceId);
        Assert.NotNull(result.Exception);
        Assert.IsType<OperationCanceledException>(result.Exception);
    }

    #endregion

    #region Functional Transformation Tests

    [Fact]
    public void Map_WhenInputIsSuccess_MapsValue_AndPreservesTraceId()
    {
        // Arrange
        var service = new TestService("trace-map");
        var input = ServiceResult<int>.Ok(12, "ok").WithTraceId("input-trace");

        // Act
        var result = service.CallMap(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-map", result.TraceId); // DefaultTraceId is applied on Ok()
        Assert.Equal("12", result.Value);
    }

    [Fact]
    public void Map_WhenInputIsSuccess_WithCustomMessage_UsesCustomMessage()
    {
        // Arrange
        var service = new TestService("trace-map-msg");
        var input = ServiceResult<int>.Ok(42, "original message");

        // Act
        var result = service.CallMap(input, "custom message");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("custom message", result.Message);
        Assert.Equal("42", result.Value);
    }

    [Fact]
    public void Map_WhenInputIsFailure_PreservesFailure_AndAppliesTraceId()
    {
        // Arrange
        var service = new TestService("trace-map-f");
        ServiceResult<int> input = (ServiceResult<int>)ServiceResult.NotFound("missing").WithTraceId("input-trace");

        // Act
        var result = service.CallMap(input);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-map-f", result.TraceId); // wrapper applies DefaultTraceId
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public void Map_WhenInputIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new TestService("trace-map-null");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.CallMap(null!));
    }

    [Fact]
    public void Bind_WhenInputIsSuccess_AndBindSucceeds_ReturnsBoundResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-bind");
        var input = ServiceResult<int>.Ok(5).WithTraceId("input-trace");

        // Act
        var result = service.CallBind(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-bind", result.TraceId);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public void Bind_WhenInputIsSuccess_ButBindReturnsFailure_PropagatesFailure_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-bind-f");
        var input = ServiceResult<int>.Ok(-1).WithTraceId("input-trace");

        // Act
        var result = service.CallBind(input);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-bind-f", result.TraceId);
        Assert.Equal(ServiceResultStatus.Failure, result.Status);
    }

    [Fact]
    public void Bind_WhenInputIsFailure_PreservesFailure_AndAppliesTraceId()
    {
        // Arrange
        var service = new TestService("trace-bind-preserve");
        var input = ServiceResult<int>.FromNonGeneric(ServiceResult.NotFound("not found"));

        // Act
        var result = service.CallBind(input);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-bind-preserve", result.TraceId);
        Assert.Equal(ServiceResultStatus.NotFound, result.Status);
    }

    [Fact]
    public void Bind_WhenInputIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new TestService("trace-bind-null");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.CallBind(null!));
    }

    #endregion

    #region Validation Helper Tests

    [Fact]
    public void Ensure_WhenConditionIsTrue_ReturnsSuccessResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-ensure-true");

        // Act
        var result = service.CallEnsure(true, "should not fail");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-ensure-true", result.TraceId);
    }

    [Fact]
    public void Ensure_WhenConditionIsFalse_ReturnsFailureResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-ensure-false");

        // Act
        var result = service.CallEnsure(false, "condition failed", code: "CONDITION_FAILED");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-ensure-false", result.TraceId);
        Assert.Equal("CONDITION_FAILED", result.ErrorCode);
        Assert.Equal("condition failed", result.Message);
    }

    [Fact]
    public void EnsureT_WhenConditionIsTrue_ReturnsSuccessResult_WithDefaultValue_AndTraceId()
    {
        // Arrange
        var service = new TestService("trace-ensure-t-true");

        // Act
        var result = service.CallEnsureT(true, "should not fail");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("trace-ensure-t-true", result.TraceId);
        Assert.Equal(default(int), result.Value);
    }

    [Fact]
    public void EnsureT_WhenConditionIsFalse_ReturnsFailureResult_WithTraceId()
    {
        // Arrange
        var service = new TestService("trace-ensure-t-false");

        // Act
        var result = service.CallEnsureT(false, "condition failed", code: "CONDITION_FAILED");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("trace-ensure-t-false", result.TraceId);
        Assert.Equal("CONDITION_FAILED", result.ErrorCode);
    }

    [Fact]
    public void Ensure_WhenMessageIsNullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var service = new TestService("trace-ensure-null");

        // Act & Assert
        // ThrowIfNullOrWhiteSpace throws ArgumentNullException for null, ArgumentException for empty/whitespace
        Assert.Throws<ArgumentNullException>(() => service.CallEnsure(false, null!));
        Assert.Throws<ArgumentException>(() => service.CallEnsure(false, ""));
        Assert.Throws<ArgumentException>(() => service.CallEnsure(false, "   "));
    }

    #endregion

    #region Trace ID Tests

    [Fact]
    public void DefaultTraceId_WhenNull_ResultsHaveNullTraceId()
    {
        // Arrange
        var service = new TestService(null);

        // Act
        var result = service.CallOk("test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.TraceId);
    }

    [Fact]
    public void DefaultTraceId_WhenSet_AllResultsUseSameTraceId()
    {
        // Arrange
        const string traceId = "consistent-trace-id";
        var service = new TestService(traceId);

        // Act
        var okResult = service.CallOk("ok");
        var failResult = service.CallFail("fail");
        var notFoundResult = service.CallNotFound("not found");

        // Assert
        Assert.Equal(traceId, okResult.TraceId);
        Assert.Equal(traceId, failResult.TraceId);
        Assert.Equal(traceId, notFoundResult.TraceId);
    }

    #endregion
}
