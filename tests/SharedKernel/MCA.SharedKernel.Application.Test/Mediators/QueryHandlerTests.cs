/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Tests
 * QueryHandler configuration tests.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using AkbarAmd.SharedKernel.Application.Mediators;
using MediatR;

namespace MCA.SharedKernel.Application.Test.Mediators;

public class QueryHandlerTests
{
    [Fact]
    public void QueryHandler_EnableCache_ShouldSetCacheConfiguration()
    {
        // Arrange
        var handler = new TestQueryHandlerWithCache();

        // Act
        var config = ((IQueryHandlerConfiguration)handler).GetCacheConfiguration();

        // Assert
        Assert.True(config.Enabled);
        Assert.Equal(10, config.DurationMinutes);
    }

    [Fact]
    public void QueryHandler_DisableCache_ShouldDisableCache()
    {
        // Arrange
        var handler = new TestQueryHandlerWithoutCache();
        var config = ((IQueryHandlerConfiguration)handler).GetCacheConfiguration();

        // Assert
        Assert.False(config.Enabled);
    }

    [Fact]
    public void QueryHandler_EnablePerformanceTracking_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestQueryHandlerWithPerformanceTracking();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnablePerformanceTracking);
    }

    [Fact]
    public void QueryHandler_EnableRetryPolicy_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestQueryHandlerWithRetry();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(config.EnableRetryPolicy);
        Assert.Equal(5, config.MaxRetryAttempts);
        Assert.Equal(2000, config.RetryDelayMs);
    }

    [Fact]
    public void QueryHandler_SetTimeout_ShouldSetConfiguration()
    {
        // Arrange
        var handler = new TestQueryHandlerWithTimeout();

        // Act
        var config = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.Equal(30, config.TimeoutSeconds);
    }

    [Fact]
    public void QueryHandler_ConfigureBehavior_ShouldSetFullConfiguration()
    {
        // Arrange
        var handler = new TestQueryHandlerWithFullConfig();

        // Act
        var retrievedConfig = ((IHandlerBehaviorConfiguration)handler).GetBehaviorConfiguration();

        // Assert
        Assert.True(retrievedConfig.EnableDetailedLogging);
        Assert.True(retrievedConfig.EnablePerformanceTracking);
        Assert.True(retrievedConfig.EnableRetryPolicy);
    }

    // Test handlers
    public class TestQueryHandlerWithCache : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithCache()
        {
            EnableCache(durationMinutes: 10);
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQueryHandlerWithoutCache : QueryHandler<TestQuery, TestResult>
    {
        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQueryHandlerWithPerformanceTracking : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithPerformanceTracking()
        {
            EnablePerformanceTracking();
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQueryHandlerWithRetry : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithRetry()
        {
            EnableRetryPolicy(maxAttempts: 5, delayMs: 2000);
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQueryHandlerWithTimeout : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithTimeout()
        {
            SetTimeout(30);
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQueryHandlerWithFullConfig : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithFullConfig()
        {
            ConfigureBehavior(HandlerBehaviorConfiguration.Full());
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "test" });
        }
    }

    public class TestQuery : IQuery<TestResult>
    {
        public int Id { get; set; }
    }

    public class TestResult
    {
        public string Value { get; set; } = string.Empty;
    }
}

