/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediator Contract
 * Abstraction for mediator pattern implementation.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Application.Contracts;

/// <summary>
/// CQRS mediator interface for sending commands, queries, and publishing events.
/// Provides a clean abstraction over the mediator pattern implementation with caching support.
/// Aligns with CQRS (Command Query Responsibility Segregation) principles.
/// </summary>
public interface ICqrsMediator
{
    /// <summary>
    /// Sends a command that modifies system state and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the command processing.</returns>
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command that modifies system state without returning a result.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Send(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query that retrieves data without modifying system state.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the query.</typeparam>
    /// <param name="query">The query to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the query processing.</returns>
    Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes an event to all registered handlers.
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Publish(IEvent @event, CancellationToken cancellationToken = default);
}

