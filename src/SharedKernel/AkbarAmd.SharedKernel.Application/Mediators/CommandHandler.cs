/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * CommandHandler base classes for command processing with DDD alignment.
 * Year: 2025
 */

using MediatR;

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling commands that modify system state.
/// Commands represent intentions to change the system state and should be handled
/// within the application layer following CQRS principles.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public abstract class CommandHandler<TCommand> : IRequestHandler<TCommand>, IHandlerBehaviorConfiguration
    where TCommand : IRequest
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
    /// Initializes a new instance of the CommandHandler class.
    /// </summary>
    protected CommandHandler()
    {
        BehaviorConfiguration = HandlerBehaviorConfiguration.Default();
    }
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

    #region Behavior Configuration Methods

    /// <summary>
    /// Enables detailed logging for this command handler.
    /// </summary>
    protected void EnableDetailedLogging()
    {
        BehaviorConfiguration.EnableDetailedLogging = true;
    }

    /// <summary>
    /// Enables performance tracking for this command handler.
    /// </summary>
    protected void EnablePerformanceTracking()
    {
        BehaviorConfiguration.EnablePerformanceTracking = true;
    }

    /// <summary>
    /// Enables retry policy for this command handler.
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
    /// Enables transaction management for this command handler.
    /// </summary>
    protected void EnableTransaction()
    {
        BehaviorConfiguration.EnableTransaction = true;
    }

    /// <summary>
    /// Sets a timeout for command execution.
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

/// <summary>
/// Base class for handling commands that modify system state and return a result.
/// Commands represent intentions to change the system state and should be handled
/// within the application layer following CQRS principles.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the command.</typeparam>
public abstract class CommandHandler<TCommand, TResult> : IRequestHandler<TCommand, TResult>, IHandlerBehaviorConfiguration
    where TCommand : IRequest<TResult>
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
    /// Initializes a new instance of the CommandHandler class.
    /// </summary>
    protected CommandHandler()
    {
        BehaviorConfiguration = HandlerBehaviorConfiguration.Default();
    }
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

    #region Behavior Configuration Methods

    /// <summary>
    /// Enables detailed logging for this command handler.
    /// </summary>
    protected void EnableDetailedLogging()
    {
        BehaviorConfiguration.EnableDetailedLogging = true;
    }

    /// <summary>
    /// Enables performance tracking for this command handler.
    /// </summary>
    protected void EnablePerformanceTracking()
    {
        BehaviorConfiguration.EnablePerformanceTracking = true;
    }

    /// <summary>
    /// Enables retry policy for this command handler.
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
    /// Enables transaction management for this command handler.
    /// </summary>
    protected void EnableTransaction()
    {
        BehaviorConfiguration.EnableTransaction = true;
    }

    /// <summary>
    /// Sets a timeout for command execution.
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