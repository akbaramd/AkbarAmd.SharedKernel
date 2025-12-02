/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * EventHandler base class for integration events and application-level events.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using MediatR;

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling integration events and application-level events.
/// Integration events are used for cross-bounded context communication
/// and external system integration after business operations are completed.
/// </summary>
/// <typeparam name="TEvent">The type of integration event to handle.</typeparam>
public abstract class EventHandler<TEvent> : INotificationHandler<TEvent>, IHandlerBehaviorConfiguration
    where TEvent : IEvent
{
    /// <summary>
    /// Protected field for behavior configuration. Can be configured using behavior methods.
    /// </summary>
    protected HandlerBehaviorConfiguration BehaviorConfiguration { get; private set; }

    /// <summary>
    /// Internal method to get behavior configuration for mediator access.
    /// </summary>
    HandlerBehaviorConfiguration IHandlerBehaviorConfiguration.GetBehaviorConfiguration() => BehaviorConfiguration;

    /// <summary>
    /// Initializes a new instance of the EventHandler class.
    /// </summary>
    protected EventHandler()
    {
        BehaviorConfiguration = HandlerBehaviorConfiguration.Default();
    }
    /// <summary>
    /// Protected method for handling the integration event asynchronously.
    /// Override this method to implement specific event handling logic.
    /// </summary>
    /// <param name="notification">The integration event to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
    protected abstract Task ProcessAsync(TEvent notification, CancellationToken cancellationToken);

    /// <summary>
    /// Public processor method that wraps the protected implementation with cross-cutting concerns.
    /// This method is called by MediatR and provides logging, validation, and error handling.
    /// </summary>
    /// <param name="notification">The integration event to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        try
        {
            // Log event received
            await OnEventReceived(notification);

            // Validate integration event
            await ValidateIntegrationEvent(notification);

            // Process the integration event using protected method
            await ProcessAsync(notification, cancellationToken);

            // Log event processed successfully
            await OnEventProcessed(notification);
        }
        catch (Exception ex)
        {
            // Log event processing error
            await OnEventError(notification, ex);
            throw;
        }
    }

    /// <summary>
    /// Called when an integration event is received.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="notification">The integration event that was received.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnEventReceived(TEvent notification)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the integration event before processing.
    /// Override to add custom validation logic for integration events.
    /// </summary>
    /// <param name="notification">The integration event to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ValidateIntegrationEvent(TEvent notification)
    {
        // Default implementation - can be overridden for custom validation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an integration event has been processed successfully.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="notification">The integration event that was processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnEventProcessed(TEvent notification)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs while processing an integration event.
    /// Override to add custom error logging or error handling.
    /// </summary>
    /// <param name="notification">The integration event that caused the error.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnEventError(TEvent notification, Exception exception)
    {
        // Default implementation - can be overridden for custom error handling
        return Task.CompletedTask;
    }

    #region Behavior Configuration Methods

    /// <summary>
    /// Enables detailed logging for this event handler.
    /// </summary>
    protected void EnableDetailedLogging()
    {
        BehaviorConfiguration.EnableDetailedLogging = true;
    }

    /// <summary>
    /// Enables performance tracking for this event handler.
    /// </summary>
    protected void EnablePerformanceTracking()
    {
        BehaviorConfiguration.EnablePerformanceTracking = true;
    }

    /// <summary>
    /// Enables retry policy for this event handler.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts. Default is 3.</param>
    /// <param name="delayMs">Delay between retries in milliseconds. Default is 1000ms.</param>
    protected void EnableRetryPolicy(int maxAttempts = 3, int delayMs = 1000)
    {
        BehaviorConfiguration.EnableRetryPolicy = true;
        BehaviorConfiguration.MaxRetryAttempts = maxAttempts;
        BehaviorConfiguration.RetryDelayMs = delayMs;
    }

    /// <summary>
    /// Sets a timeout for event processing.
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    protected void SetTimeout(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
            throw new ArgumentException("Timeout must be greater than zero.", nameof(timeoutSeconds));
        
        BehaviorConfiguration.TimeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// Configures behavior settings using a configuration object.
    /// </summary>
    /// <param name="configuration">The behavior configuration.</param>
    protected void ConfigureBehavior(HandlerBehaviorConfiguration configuration)
    {
        BehaviorConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    #endregion
}

