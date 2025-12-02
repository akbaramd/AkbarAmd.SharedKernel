/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Tests
 * CommandHandler configuration tests.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Mediators;
using MediatR;

namespace MCA.SharedKernel.Application.Test.Mediators;

public class CommandHandlerTests
{
    [Fact]
    public void CommandHandler_EnableTransaction_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestCommandHandlerWithTransaction();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnableTransaction);
    }

    [Fact]
    public void CommandHandler_EnableRetryPolicy_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestCommandHandlerWithRetry();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnableRetryPolicy);
        Assert.Equal(3, config.MaxRetryAttempts);
        Assert.Equal(500, config.RetryDelayMs);
    }

    [Fact]
    public void CommandHandler_EnablePerformanceTracking_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestCommandHandlerWithPerformanceTracking();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnablePerformanceTracking);
    }

    [Fact]
    public void CommandHandler_WithResult_EnableTransaction_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestCommandHandlerWithResultAndTransaction();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnableTransaction);
    }

    // Test handlers
    public class TestCommandHandlerWithTransaction : CommandHandler<TestCommand>
    {
        public TestCommandHandlerWithTransaction()
        {
            EnableTransaction();
        }

        protected override Task ProcessAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCommandHandlerWithRetry : CommandHandler<TestCommand>
    {
        public TestCommandHandlerWithRetry()
        {
            EnableRetryPolicy(maxAttempts: 3, delayMs: 500);
        }

        protected override Task ProcessAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCommandHandlerWithPerformanceTracking : CommandHandler<TestCommand>
    {
        public TestCommandHandlerWithPerformanceTracking()
        {
            EnablePerformanceTracking();
        }

        protected override Task ProcessAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestCommandHandlerWithResultAndTransaction : CommandHandler<TestCommandWithResult, Guid>
    {
        public TestCommandHandlerWithResultAndTransaction()
        {
            EnableTransaction();
        }

        protected override Task<Guid> ProcessAsync(TestCommandWithResult request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }

    public class TestCommand : IRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandWithResult : IRequest<Guid>
    {
        public string Value { get; set; } = string.Empty;
    }
}

