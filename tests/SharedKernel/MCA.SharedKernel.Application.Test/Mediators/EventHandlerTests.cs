/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Tests
 * EventHandler configuration tests.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using AkbarAmd.SharedKernel.Application.Mediators;
using MediatR;

namespace MCA.SharedKernel.Application.Test.Mediators;

public class EventHandlerTests
{
    [Fact]
    public void EventHandler_EnableRetryPolicy_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestEventHandlerWithRetry();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnableRetryPolicy);
        Assert.Equal(5, config.MaxRetryAttempts);
        Assert.Equal(1000, config.RetryDelayMs);
    }

    [Fact]
    public void EventHandler_EnablePerformanceTracking_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestEventHandlerWithPerformanceTracking();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnablePerformanceTracking);
    }

    [Fact]
    public void EventHandler_SetTimeout_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestEventHandlerWithTimeout();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.Equal(60, config.TimeoutSeconds);
    }

    // Test handlers
    public class TestEventHandlerWithRetry : AkbarAmd.SharedKernel.Application.Mediators.EventHandler<TestEvent>
    {
        public TestEventHandlerWithRetry()
        {
            EnableRetryPolicy(maxAttempts: 5, delayMs: 1000);
        }

        protected override Task ProcessAsync(TestEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestEventHandlerWithPerformanceTracking : AkbarAmd.SharedKernel.Application.Mediators.EventHandler<TestEvent>
    {
        public TestEventHandlerWithPerformanceTracking()
        {
            EnablePerformanceTracking();
        }

        protected override Task ProcessAsync(TestEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestEventHandlerWithTimeout : AkbarAmd.SharedKernel.Application.Mediators.EventHandler<TestEvent>
    {
        public TestEventHandlerWithTimeout()
        {
            SetTimeout(60);
        }

        protected override Task ProcessAsync(TestEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class TestEvent : IEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }
        public string Source { get; } = "Test";
    }
}

