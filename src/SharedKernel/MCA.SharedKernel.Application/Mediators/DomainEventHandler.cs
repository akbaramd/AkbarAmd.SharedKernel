/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * DomainEventHandler base class for domain events within the same bounded context.
 * Year: 2025
 */

using MCA.SharedKernel.Application.Contracts;
using MediatR;

namespace MCA.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling domain events within the same bounded context.
/// Domain events represent state changes that have occurred within aggregates
/// and are used for internal communication within the bounded context.
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle.</typeparam>
public abstract class DomainEventHandler<TEvent> : INotificationHandler<TEvent> 
    where TEvent : class, MediatR.INotification
{
    /// <summary>
    /// Protected method for handling the domain event asynchronously.
    /// Override this method to implement specific domain event handling logic.
    /// </summary>
    /// <param name="notification">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when notification is null.</exception>
    protected abstract Task ProcessAsync(TEvent notification, CancellationToken cancellationToken);

    /// <summary>
    /// Public processor method that wraps the protected implementation with cross-cutting concerns.
    /// This method is called by MediatR and provides logging, validation, and error handling.
    /// </summary>
    /// <param name="notification">The domain event to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        if (notification == null)
            throw new ArgumentNullException(nameof(notification));

        try
        {
            // Log domain event received
            await OnDomainEventReceived(notification);

            // Validate domain event
            await ValidateDomainEvent(notification);

            // Process the domain event using protected method
            await ProcessAsync(notification, cancellationToken);

            // Log domain event processed successfully
            await OnDomainEventProcessed(notification);
        }
        catch (Exception ex)
        {
            // Log domain event processing error
            await OnDomainEventError(notification, ex);
            throw;
        }
    }

    /// <summary>
    /// Called when a domain event is received.
    /// Override to add custom domain-specific logging or metrics.
    /// </summary>
    /// <param name="notification">The domain event that was received.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnDomainEventReceived(TEvent notification)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the domain event before processing.
    /// Override to add custom validation logic for domain events.
    /// </summary>
    /// <param name="notification">The domain event to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ValidateDomainEvent(TEvent notification)
    {
        // Default implementation - can be overridden for custom validation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a domain event has been processed successfully.
    /// Override to add custom domain-specific logging or metrics.
    /// </summary>
    /// <param name="notification">The domain event that was processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnDomainEventProcessed(TEvent notification)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs while processing a domain event.
    /// Override to add custom domain-specific error logging or error handling.
    /// </summary>
    /// <param name="notification">The domain event that caused the error.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnDomainEventError(TEvent notification, Exception exception)
    {
        // Default implementation - can be overridden for custom error handling
        return Task.CompletedTask;
    }
}

