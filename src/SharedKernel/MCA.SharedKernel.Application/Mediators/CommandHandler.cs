/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * CommandHandler base classes for command processing with DDD alignment.
 * Year: 2025
 */

using MCA.SharedKernel.Application.Contracts;
using MediatR;

namespace MCA.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling commands that modify system state.
/// Commands represent intentions to change the system state and should be handled
/// within the application layer following CQRS principles.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand> 
    where TCommand : IRequest
{
    /// <summary>
    /// Protected method for processing the command asynchronously.
    /// Override this method to implement specific command handling logic.
    /// </summary>
    /// <param name="request">The command to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    protected abstract Task ProcessAsync(TCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Public processor method that wraps the protected implementation with cross-cutting concerns.
    /// This method is called by MediatR and provides logging, validation, and error handling.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            // Log command received
            await OnCommandReceived(request);

            // Validate command
            await ValidateCommand(request);

            // Process the command using protected method
            await ProcessAsync(request, cancellationToken);

            // Log command processed successfully
            await OnCommandProcessed(request);
        }
        catch (Exception ex)
        {
            // Log command processing error
            await OnCommandError(request, ex);
            throw;
        }
    }

    /// <summary>
    /// Called when a command is received.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The command that was received.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandReceived(TCommand request)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the command before processing.
    /// Override to add custom validation logic for commands.
    /// </summary>
    /// <param name="request">The command to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ValidateCommand(TCommand request)
    {
        // Default implementation - can be overridden for custom validation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a command has been processed successfully.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The command that was processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandProcessed(TCommand request)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs while processing a command.
    /// Override to add custom error logging or error handling.
    /// </summary>
    /// <param name="request">The command that caused the error.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandError(TCommand request, Exception exception)
    {
        // Default implementation - can be overridden for custom error handling
        return Task.CompletedTask;
    }
}

/// <summary>
/// Base class for handling commands that modify system state and return a result.
/// Commands represent intentions to change the system state and should be handled
/// within the application layer following CQRS principles.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult> 
    where TCommand : IRequest<TResult>
{
    /// <summary>
    /// Protected method for processing the command asynchronously and returning a result.
    /// Override this method to implement specific command handling logic.
    /// </summary>
    /// <param name="request">The command to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the command processing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    protected abstract Task<TResult> ProcessAsync(TCommand request, CancellationToken cancellationToken);

    /// <summary>
    /// Public processor method that wraps the protected implementation with cross-cutting concerns.
    /// This method is called by MediatR and provides logging, validation, and error handling.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the command processing.</returns>
    public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            // Log command received
            await OnCommandReceived(request);

            // Validate command
            await ValidateCommand(request);

            // Process the command using protected method
            var result = await ProcessAsync(request, cancellationToken);

            // Log command processed successfully
            await OnCommandProcessed(request, result);

            return result;
        }
        catch (Exception ex)
        {
            // Log command processing error
            await OnCommandError(request, ex);
            throw;
        }
    }

    /// <summary>
    /// Called when a command is received.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The command that was received.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandReceived(TCommand request)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the command before processing.
    /// Override to add custom validation logic for commands.
    /// </summary>
    /// <param name="request">The command to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ValidateCommand(TCommand request)
    {
        // Default implementation - can be overridden for custom validation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a command has been processed successfully.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The command that was processed.</param>
    /// <param name="result">The result returned by the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandProcessed(TCommand request, TResult result)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs while processing a command.
    /// Override to add custom error logging or error handling.
    /// </summary>
    /// <param name="request">The command that caused the error.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnCommandError(TCommand request, Exception exception)
    {
        // Default implementation - can be overridden for custom error handling
        return Task.CompletedTask;
    }
}