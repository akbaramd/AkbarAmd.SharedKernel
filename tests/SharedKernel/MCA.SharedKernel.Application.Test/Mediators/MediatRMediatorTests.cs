/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Tests
 * MediatRMediator tests with mocks.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using AkbarAmd.SharedKernel.Application.Mediators;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace MCA.SharedKernel.Application.Test.Mediators;

public class MediatRMediatorTests
{
    private readonly Mock<MediatR.IMediator> _mediatorMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<MediatRMediator>> _loggerMock;
    private readonly MemoryCache _realCache;
    private readonly ServiceCollection _services;
    private readonly MediatRMediator _mediator;

    public MediatRMediatorTests()
    {
        _mediatorMock = new Mock<MediatR.IMediator>();
        _cacheMock = new Mock<IMemoryCache>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<MediatRMediator>>();
        
        // Use real memory cache for integration tests
        _realCache = new MemoryCache(new MemoryCacheOptions());
        _services = new ServiceCollection();
        _services.AddMemoryCache();
        _services.AddSingleton(_mediatorMock.Object);
        
        var serviceProvider = _services.BuildServiceProvider();
        _mediator = new MediatRMediator(
            _mediatorMock.Object,
            _realCache,
            serviceProvider,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Send_CommandWithResult_ShouldCallMediatR()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mediator.Send(command);

        // Assert
        Assert.Equal(expectedResult, result);
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_CommandWithoutResult_ShouldCallMediatR()
    {
        // Arrange
        var command = new TestCommandWithoutResult { Value = "test" };
        var handler = new TestCommandWithoutResultHandler();
        _services.AddSingleton<IRequestHandler<TestCommandWithoutResult>>(handler);
        var serviceProvider = _services.BuildServiceProvider();
        var mediator = new MediatRMediator(
            _mediatorMock.Object,
            _realCache,
            serviceProvider,
            _loggerMock.Object);

        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mediator.Send(command);

        // Assert
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Query_WithoutCache_ShouldCallMediatR()
    {
        // Arrange
        var query = new TestQuery { Id = 1 };
        var expectedResult = new TestResult { Value = "result" };
        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mediator.Query(query);

        // Assert
        Assert.Equal(expectedResult, result);
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Query_WithCache_ShouldReturnCachedResult()
    {
        // Arrange
        var query = new TestQuery { Id = 1 };
        var expectedResult = new TestResult { Value = "cached" };
        
        // Register handler with cache enabled
        var handler = new TestQueryHandlerWithCache();
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton(_mediatorMock.Object);
        services.AddSingleton<IRequestHandler<TestQuery, TestResult>>(handler);
        var serviceProvider = services.BuildServiceProvider();
        var mediator = new MediatRMediator(
            _mediatorMock.Object,
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider,
            _loggerMock.Object);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act - First call (will discover config and execute)
        var result1 = await mediator.Query(query);
        
        // Clear mock to track second call separately
        _mediatorMock.Reset();
        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult { Value = "should not be called" });
        
        // Act - Second call (should use cache, no MediatR call)
        var result2 = await mediator.Query(query);

        // Assert
        Assert.Equal(expectedResult, result1);
        Assert.Equal(expectedResult, result2);
        // Should NOT call MediatR on second call due to caching
        _mediatorMock.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Publish_Event_ShouldCallMediatR()
    {
        // Arrange
        var @event = new TestEvent();
        var handler = new TestEventHandler();
        _services.AddSingleton<INotificationHandler<TestEvent>>(handler);
        var serviceProvider = _services.BuildServiceProvider();
        var mediator = new MediatRMediator(
            _mediatorMock.Object,
            _realCache,
            serviceProvider,
            _loggerMock.Object);

        _mediatorMock.Setup(m => m.Publish(@event, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mediator.Publish(@event);

        // Assert
        _mediatorMock.Verify(m => m.Publish(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Send_CommandWithRetry_ShouldRetryOnFailure()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var handler = new TestCommandHandlerWithRetry();
        _services.AddSingleton<IRequestHandler<TestCommand, Guid>>(handler);
        var serviceProvider = _services.BuildServiceProvider();
        var mediator = new MediatRMediator(
            _mediatorMock.Object,
            _realCache,
            serviceProvider,
            _loggerMock.Object);

        int callCount = 0;
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount < 3)
                    throw new Exception("Transient error");
                return Guid.NewGuid();
            });

        // Act
        var result = await mediator.Send(command);

        // Assert
        Assert.NotNull(result);
        // Should have retried 3 times (failed twice, succeeded on third)
        _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Query_WithTimeout_ShouldThrowOnTimeout()
    {
        // Arrange
        var query = new TestQuery { Id = 1 };
        var handler = new TestQueryHandlerWithTimeout();
        _services.AddSingleton<IRequestHandler<TestQuery, TestResult>>(handler);
        var serviceProvider = _services.BuildServiceProvider();
        var mediator = new MediatRMediator(
            _mediatorMock.Object,
            _realCache,
            serviceProvider,
            _loggerMock.Object);

        _mediatorMock.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .Returns(async (TestQuery q, CancellationToken ct) =>
            {
                try
                {
                    await Task.Delay(2000, ct); // Delay longer than timeout
                }
                catch (OperationCanceledException)
                {
                    // Expected when timeout occurs
                }
                ct.ThrowIfCancellationRequested();
                return new TestResult { Value = "result" };
            });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await mediator.Query(query));
    }
    
    public class TestEventHandler : AkbarAmd.SharedKernel.Application.Mediators.EventHandler<TestEvent>
    {
        protected override Task ProcessAsync(TestEvent notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    // Test classes
    public class TestCommand : ICommand<Guid>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestCommandWithoutResult : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }
    
    public class TestCommandWithoutResultHandler : CommandHandler<TestCommandWithoutResult>
    {
        protected override Task ProcessAsync(TestCommandWithoutResult request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
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

    public class TestEvent : IEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public string? CorrelationId { get; set; }
        public string Source { get; init; } = "Test";
    }

    public class TestQueryHandlerWithCache : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithCache()
        {
            EnableCache(durationMinutes: 5);
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "processed" });
        }
    }

    public class TestQueryHandlerWithTimeout : QueryHandler<TestQuery, TestResult>
    {
        public TestQueryHandlerWithTimeout()
        {
            SetTimeout(1); // 1 second timeout
        }

        protected override Task<TestResult> ProcessAsync(TestQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TestResult { Value = "processed" });
        }
    }

    public class TestCommandHandlerWithRetry : CommandHandler<TestCommand, Guid>
    {
        public TestCommandHandlerWithRetry()
        {
            EnableRetryPolicy(maxAttempts: 3, delayMs: 100);
        }

        protected override Task<Guid> ProcessAsync(TestCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }
}

