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

        // MediatR calls Send with IRequest (ICommand implements IRequest)
        _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mediator.Send(command);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest>(), It.IsAny<CancellationToken>()), Times.Once);
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

        // MediatR calls Publish with the specific event type
        _mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mediator.Publish(@event);

        // Assert
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
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

}

