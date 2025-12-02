/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * MediatR implementation of ICqrsMediator with caching support.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// MediatR-based implementation of ICqrsMediator with in-memory caching support for queries.
/// Automatically discovers cache configurations from query handlers.
/// </summary>
public sealed class MediatRMediator : Contracts.ICqrsMediator
{
    private readonly MediatR.IMediator _mediator;
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediatRMediator>? _logger;
    private readonly Dictionary<Type, QueryCacheConfiguration> _cacheConfigurations;

    /// <summary>
    /// Initializes a new instance of the MediatRMediator class.
    /// </summary>
    /// <param name="mediator">The underlying MediatR mediator instance.</param>
    /// <param name="cache">The memory cache instance for query result caching.</param>
    /// <param name="serviceProvider">The service provider for discovering handler configurations.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="cacheConfigurations">Optional dictionary of query type to cache configuration mappings.</param>
    public MediatRMediator(
        MediatR.IMediator mediator,
        IMemoryCache cache,
        IServiceProvider serviceProvider,
        ILogger<MediatRMediator>? logger = null,
        Dictionary<Type, QueryCacheConfiguration>? cacheConfigurations = null)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
        _cacheConfigurations = cacheConfigurations ?? new Dictionary<Type, QueryCacheConfiguration>();
    }

    /// <inheritdoc />
    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var behaviorConfig = GetBehaviorConfiguration(commandType);

        return await ExecuteWithBehaviors(
            async () => await _mediator.Send(command, cancellationToken),
            commandType.Name,
            behaviorConfig,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var commandType = command.GetType();
        var behaviorConfig = GetBehaviorConfiguration(commandType);

        await ExecuteWithBehaviors(
            async () => await _mediator.Send(command, cancellationToken),
            commandType.Name,
            behaviorConfig,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TResult> Query<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var queryType = query.GetType();
        var cacheConfig = GetCacheConfiguration(queryType);

        // If caching is not enabled, directly send the query
        if (!cacheConfig.Enabled)
        {
            return await _mediator.Send(query, cancellationToken);
        }

        // Generate cache key
        var cacheKey = GenerateCacheKey(query, cacheConfig);

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out TResult? cachedResult) && cachedResult != null)
        {
            _logger?.LogDebug("Cache hit for query type {QueryType} with key {CacheKey}", queryType.Name, cacheKey);
            return cachedResult;
        }

        // Execute query with behaviors
        _logger?.LogDebug("Cache miss for query type {QueryType} with key {CacheKey}. Executing query.", queryType.Name, cacheKey);
        var behaviorConfig = GetBehaviorConfiguration(queryType);
        var result = await ExecuteWithBehaviors(
            async () => await _mediator.Send(query, cancellationToken),
            queryType.Name,
            behaviorConfig,
            cancellationToken);

        // Cache the result
        if (result != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheConfig.DurationMinutes)
            };

            _cache.Set(cacheKey, result, cacheOptions);
            _logger?.LogDebug("Cached result for query type {QueryType} with key {CacheKey} for {Duration} minutes", 
                queryType.Name, cacheKey, cacheConfig.DurationMinutes);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task Publish(IEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = @event.GetType();
        var behaviorConfig = GetBehaviorConfiguration(eventType);

        await ExecuteWithBehaviors(
            async () => await _mediator.Publish(@event, cancellationToken),
            eventType.Name,
            behaviorConfig,
            cancellationToken);
    }

    /// <summary>
    /// Registers a cache configuration for a specific query type.
    /// </summary>
    /// <typeparam name="TQuery">The type of query.</typeparam>
    /// <param name="configuration">The cache configuration.</param>
    public void RegisterCacheConfiguration<TQuery, TResult>(QueryCacheConfiguration configuration) where TQuery : IQuery<TResult>
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        _cacheConfigurations[typeof(TQuery)] = configuration;
    }

    /// <summary>
    /// Gets the cache configuration for a query type.
    /// First checks explicit registrations, then discovers from handler if available.
    /// </summary>
    /// <param name="queryType">The type of query.</param>
    /// <returns>The cache configuration, or a disabled configuration if not found.</returns>
    private QueryCacheConfiguration GetCacheConfiguration(Type queryType)
    {
        // First check explicit registrations
        if (_cacheConfigurations.TryGetValue(queryType, out var config))
        {
            return config;
        }

        // Try to discover configuration from handler
        try
        {
            // Get the handler type for this query
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(queryType, GetQueryResultType(queryType));
            var handler = _serviceProvider.GetService(handlerType);
            
            if (handler is IQueryHandlerConfiguration handlerConfig)
            {
                var discoveredConfig = handlerConfig.GetCacheConfiguration();
                // Cache it for future use
                _cacheConfigurations[queryType] = discoveredConfig;
                return discoveredConfig;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Could not discover cache configuration for query type {QueryType}", queryType.Name);
        }

        return QueryCacheConfiguration.Disable();
    }

    /// <summary>
    /// Gets the result type from a query type that implements IQuery&lt;TResult&gt;.
    /// </summary>
    /// <param name="queryType">The query type.</param>
    /// <returns>The result type, or object if not found.</returns>
    private static Type GetQueryResultType(Type queryType)
    {
        var queryInterface = queryType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
        
        return queryInterface?.GetGenericArguments()[0] ?? typeof(object);
    }

    /// <summary>
    /// Generates a cache key for a query.
    /// </summary>
    /// <typeparam name="TResult">The type of query result.</typeparam>
    /// <param name="query">The query instance.</param>
    /// <param name="config">The cache configuration.</param>
    /// <returns>A cache key string.</returns>
    private string GenerateCacheKey<TResult>(IQuery<TResult> query, QueryCacheConfiguration config)
    {
        if (config.CacheKeyGenerator != null)
        {
            return config.CacheKeyGenerator(query);
        }

        // Default cache key generation: query type name + serialized query parameters
        var queryType = query.GetType();
        var queryHash = JsonSerializer.Serialize(query).GetHashCode();
        return $"Query:{queryType.Name}:{queryHash}";
    }

    /// <summary>
    /// Gets the behavior configuration for a handler type.
    /// </summary>
    /// <param name="requestType">The type of request (command, query, or event).</param>
    /// <returns>The behavior configuration, or default if not found.</returns>
    private HandlerBehaviorConfiguration GetBehaviorConfiguration(Type requestType)
    {
        try
        {
            // Try to discover configuration from handler
            // For commands/queries, we need to find the handler
            var interfaces = requestType.GetInterfaces();
            
            // Check if it's a query
            var queryInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));
            if (queryInterface != null)
            {
                var resultType = queryInterface.GetGenericArguments()[0];
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                var handler = _serviceProvider.GetService(handlerType);
                
                if (handler is IHandlerBehaviorConfiguration behaviorConfig)
                {
                    return behaviorConfig.GetBehaviorConfiguration();
                }
            }
            
            // Check if it's a command with result
            var commandWithResultInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
            if (commandWithResultInterface != null)
            {
                var resultType = commandWithResultInterface.GetGenericArguments()[0];
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, resultType);
                var handler = _serviceProvider.GetService(handlerType);
                
                if (handler is IHandlerBehaviorConfiguration behaviorConfig)
                {
                    return behaviorConfig.GetBehaviorConfiguration();
                }
            }
            
            // Check if it's a command without result
            if (interfaces.Any(i => i == typeof(ICommand)))
            {
                var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
                var handler = _serviceProvider.GetService(handlerType);
                
                if (handler is IHandlerBehaviorConfiguration behaviorConfig)
                {
                    return behaviorConfig.GetBehaviorConfiguration();
                }
            }
            
            // Check if it's an event
            if (interfaces.Any(i => i == typeof(IEvent)))
            {
                var handlerType = typeof(INotificationHandler<>).MakeGenericType(requestType);
                var handler = _serviceProvider.GetService(handlerType);
                
                if (handler is IHandlerBehaviorConfiguration behaviorConfig)
                {
                    return behaviorConfig.GetBehaviorConfiguration();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Could not discover behavior configuration for type {RequestType}", requestType.Name);
        }

        return HandlerBehaviorConfiguration.Default();
    }

    /// <summary>
    /// Executes an operation with applied behaviors (retry, timeout, performance tracking, logging).
    /// </summary>
    /// <typeparam name="TResult">The type of result.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="behaviorConfig">The behavior configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    private async Task<TResult> ExecuteWithBehaviors<TResult>(
        Func<Task<TResult>> operation,
        string operationName,
        HandlerBehaviorConfiguration behaviorConfig,
        CancellationToken cancellationToken)
    {
        // Create combined cancellation token with timeout if configured
        using var timeoutCts = behaviorConfig.TimeoutSeconds.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (timeoutCts != null && behaviorConfig.TimeoutSeconds.HasValue)
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(behaviorConfig.TimeoutSeconds.Value));
        }

        var effectiveCancellationToken = timeoutCts?.Token ?? cancellationToken;

        // Performance tracking
        var startTime = behaviorConfig.EnablePerformanceTracking ? DateTime.UtcNow : (DateTime?)null;

        // Detailed logging
        if (behaviorConfig.EnableDetailedLogging)
        {
            _logger?.LogInformation("Executing {OperationName} with behaviors: Retry={Retry}, Timeout={Timeout}s, PerformanceTracking={PerformanceTracking}",
                operationName,
                behaviorConfig.EnableRetryPolicy,
                behaviorConfig.TimeoutSeconds,
                behaviorConfig.EnablePerformanceTracking);
        }

        // Retry logic
        if (behaviorConfig.EnableRetryPolicy)
        {
            Exception? lastException = null;
            for (int attempt = 1; attempt <= behaviorConfig.MaxRetryAttempts; attempt++)
            {
                try
                {
                    var result = await operation();
                    
                    if (behaviorConfig.EnablePerformanceTracking && startTime.HasValue)
                    {
                        var duration = DateTime.UtcNow - startTime.Value;
                        _logger?.LogInformation("{OperationName} completed in {Duration}ms (attempt {Attempt})",
                            operationName, duration.TotalMilliseconds, attempt);
                    }
                    
                    if (behaviorConfig.EnableDetailedLogging && attempt > 1)
                    {
                        _logger?.LogInformation("{OperationName} succeeded on attempt {Attempt}", operationName, attempt);
                    }
                    
                    return result;
                }
                catch (Exception ex) when (attempt < behaviorConfig.MaxRetryAttempts)
                {
                    lastException = ex;
                    if (behaviorConfig.EnableDetailedLogging)
                    {
                        _logger?.LogWarning(ex, "{OperationName} failed on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms...",
                            operationName, attempt, behaviorConfig.MaxRetryAttempts, behaviorConfig.RetryDelayMs);
                    }
                    
                    await Task.Delay(behaviorConfig.RetryDelayMs, effectiveCancellationToken);
                }
            }
            
            // All retries failed
            if (behaviorConfig.EnableDetailedLogging)
            {
                _logger?.LogError(lastException, "{OperationName} failed after {MaxAttempts} attempts", operationName, behaviorConfig.MaxRetryAttempts);
            }
            
            throw lastException ?? new InvalidOperationException($"{operationName} failed after {behaviorConfig.MaxRetryAttempts} attempts");
        }
        else
        {
            // No retry - execute once
            try
            {
                var result = await operation();
                
                if (behaviorConfig.EnablePerformanceTracking && startTime.HasValue)
                {
                    var duration = DateTime.UtcNow - startTime.Value;
                    _logger?.LogInformation("{OperationName} completed in {Duration}ms", operationName, duration.TotalMilliseconds);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                if (behaviorConfig.EnableDetailedLogging)
                {
                    _logger?.LogError(ex, "{OperationName} failed", operationName);
                }
                throw;
            }
        }
    }

    /// <summary>
    /// Executes an operation with applied behaviors (retry, timeout, performance tracking, logging).
    /// Overload for operations that return Task (no result).
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="behaviorConfig">The behavior configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteWithBehaviors(
        Func<Task> operation,
        string operationName,
        HandlerBehaviorConfiguration behaviorConfig,
        CancellationToken cancellationToken)
    {
        // Create combined cancellation token with timeout if configured
        using var timeoutCts = behaviorConfig.TimeoutSeconds.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : null;

        if (timeoutCts != null && behaviorConfig.TimeoutSeconds.HasValue)
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(behaviorConfig.TimeoutSeconds.Value));
        }

        var effectiveCancellationToken = timeoutCts?.Token ?? cancellationToken;

        // Performance tracking
        var startTime = behaviorConfig.EnablePerformanceTracking ? DateTime.UtcNow : (DateTime?)null;

        // Detailed logging
        if (behaviorConfig.EnableDetailedLogging)
        {
            _logger?.LogInformation("Executing {OperationName} with behaviors: Retry={Retry}, Timeout={Timeout}s, PerformanceTracking={PerformanceTracking}",
                operationName,
                behaviorConfig.EnableRetryPolicy,
                behaviorConfig.TimeoutSeconds,
                behaviorConfig.EnablePerformanceTracking);
        }

        // Retry logic
        if (behaviorConfig.EnableRetryPolicy)
        {
            Exception? lastException = null;
            for (int attempt = 1; attempt <= behaviorConfig.MaxRetryAttempts; attempt++)
            {
                try
                {
                    await operation();
                    
                    if (behaviorConfig.EnablePerformanceTracking && startTime.HasValue)
                    {
                        var duration = DateTime.UtcNow - startTime.Value;
                        _logger?.LogInformation("{OperationName} completed in {Duration}ms (attempt {Attempt})",
                            operationName, duration.TotalMilliseconds, attempt);
                    }
                    
                    if (behaviorConfig.EnableDetailedLogging && attempt > 1)
                    {
                        _logger?.LogInformation("{OperationName} succeeded on attempt {Attempt}", operationName, attempt);
                    }
                    
                    return;
                }
                catch (Exception ex) when (attempt < behaviorConfig.MaxRetryAttempts)
                {
                    lastException = ex;
                    if (behaviorConfig.EnableDetailedLogging)
                    {
                        _logger?.LogWarning(ex, "{OperationName} failed on attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms...",
                            operationName, attempt, behaviorConfig.MaxRetryAttempts, behaviorConfig.RetryDelayMs);
                    }
                    
                    await Task.Delay(behaviorConfig.RetryDelayMs, effectiveCancellationToken);
                }
            }
            
            // All retries failed
            if (behaviorConfig.EnableDetailedLogging)
            {
                _logger?.LogError(lastException, "{OperationName} failed after {MaxAttempts} attempts", operationName, behaviorConfig.MaxRetryAttempts);
            }
            
            throw lastException ?? new InvalidOperationException($"{operationName} failed after {behaviorConfig.MaxRetryAttempts} attempts");
        }
        else
        {
            // No retry - execute once
            try
            {
                await operation();
                
                if (behaviorConfig.EnablePerformanceTracking && startTime.HasValue)
                {
                    var duration = DateTime.UtcNow - startTime.Value;
                    _logger?.LogInformation("{OperationName} completed in {Duration}ms", operationName, duration.TotalMilliseconds);
                }
            }
            catch (Exception ex)
            {
                if (behaviorConfig.EnableDetailedLogging)
                {
                    _logger?.LogError(ex, "{OperationName} failed", operationName);
                }
                throw;
            }
        }
    }
}

