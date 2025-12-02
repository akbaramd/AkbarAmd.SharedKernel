/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * QueryHandler base class for query processing with DDD alignment.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using MediatR;

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling queries that retrieve data without modifying system state.
/// Queries represent read operations and should be handled within the application layer
/// following CQRS principles. Queries should be idempotent and not cause side effects.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public abstract class QueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult>, IQueryHandlerConfiguration, IHandlerBehaviorConfiguration
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Protected field for cache configuration. Can be configured using EnableCache/DisableCache methods.
    /// </summary>
    protected QueryCacheConfiguration CacheConfiguration { get; private set; }

    /// <summary>
    /// Protected field for behavior configuration. Can be configured using behavior methods.
    /// </summary>
    protected HandlerBehaviorConfiguration BehaviorConfiguration { get; private set; }

    /// <summary>
    /// Internal method to get cache configuration for mediator access.
    /// </summary>
    QueryCacheConfiguration IQueryHandlerConfiguration.GetCacheConfiguration() => CacheConfiguration;

    /// <summary>
    /// Internal method to get behavior configuration for mediator access.
    /// </summary>
    HandlerBehaviorConfiguration IHandlerBehaviorConfiguration.GetBehaviorConfiguration() => BehaviorConfiguration;

    /// <summary>
    /// Initializes a new instance of the QueryHandler class.
    /// </summary>
    protected QueryHandler()
    {
        CacheConfiguration = QueryCacheConfiguration.Disable();
        BehaviorConfiguration = HandlerBehaviorConfiguration.Default();
    }

    /// <summary>
    /// Protected method for processing the query asynchronously and returning a result.
    /// Override this method to implement specific query handling logic.
    /// </summary>
    /// <param name="request">The query to process.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the query processing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    protected abstract Task<TResult> ProcessAsync(TQuery request, CancellationToken cancellationToken);

    /// <summary>
    /// Public processor method that wraps the protected implementation with cross-cutting concerns.
    /// This method is called by MediatR and provides logging, validation, caching, and error handling.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task containing the result of the query processing.</returns>
    public async Task<TResult> Handle(TQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            // Log query received
            await OnQueryReceived(request);

            // Validate query
            await ValidateQuery(request);

            // Check cache (if enabled via configuration)
            if (CacheConfiguration.Enabled)
            {
                var cachedResult = await TryGetFromCache(request);
                if (cachedResult != null)
                {
                    await OnQueryProcessedFromCache(request, cachedResult);
                    return cachedResult;
                }
            }

            // Process the query using protected method
            var result = await ProcessAsync(request, cancellationToken);

            // Cache result (if enabled via configuration)
            if (CacheConfiguration.Enabled)
            {
                await CacheResult(request, result);
            }

            // Log query processed successfully
            await OnQueryProcessed(request, result);

            return result;
        }
        catch (Exception ex)
        {
            // Log query processing error
            await OnQueryError(request, ex);
            throw;
        }
    }

    /// <summary>
    /// Called when a query is received.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The query that was received.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnQueryReceived(TQuery request)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the query before processing.
    /// Override to add custom validation logic for queries.
    /// </summary>
    /// <param name="request">The query to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task ValidateQuery(TQuery request)
    {
        // Default implementation - can be overridden for custom validation
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to retrieve the result from cache.
    /// Override to implement custom caching logic.
    /// </summary>
    /// <param name="request">The query to check in cache.</param>
    /// <returns>The cached result if available, otherwise null.</returns>
    protected virtual Task<TResult?> TryGetFromCache(TQuery request)
    {
        // Default implementation - no caching
        return Task.FromResult<TResult?>(default);
    }

    /// <summary>
    /// Caches the query result for future use.
    /// Override to implement custom caching logic.
    /// </summary>
    /// <param name="request">The query that was processed.</param>
    /// <param name="result">The result to cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task CacheResult(TQuery request, TResult result)
    {
        // Default implementation - no caching
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a query has been processed successfully.
    /// Override to add custom logging or metrics.
    /// </summary>
    /// <param name="request">The query that was processed.</param>
    /// <param name="result">The result returned by the query.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnQueryProcessed(TQuery request, TResult result)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a query result is retrieved from cache.
    /// Override to add custom logging or metrics for cache hits.
    /// </summary>
    /// <param name="request">The query that was processed.</param>
    /// <param name="result">The result retrieved from cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnQueryProcessedFromCache(TQuery request, TResult result)
    {
        // Default implementation - can be overridden for custom logging
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when an error occurs while processing a query.
    /// Override to add custom error logging or error handling.
    /// </summary>
    /// <param name="request">The query that caused the error.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task OnQueryError(TQuery request, Exception exception)
    {
        // Default implementation - can be overridden for custom error handling
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a cache key for the query.
    /// Uses the configured cache key generator if available, otherwise generates a default key.
    /// </summary>
    /// <param name="request">The query to generate a cache key for.</param>
    /// <returns>A cache key string.</returns>
    protected virtual string GetCacheKey(TQuery request)
    {
        if (CacheConfiguration.CacheKeyGenerator != null)
        {
            return CacheConfiguration.CacheKeyGenerator(request);
        }

        // Default implementation - use query type and hash
        return $"{typeof(TQuery).Name}_{request.GetHashCode()}";
    }

    /// <summary>
    /// Gets the cache duration for the query result from configuration.
    /// </summary>
    /// <returns>The cache duration in minutes.</returns>
    protected int GetCacheDurationMinutes()
    {
        return CacheConfiguration.DurationMinutes;
    }

    /// <summary>
    /// Enables caching for this query handler.
    /// Call this method in the constructor of derived classes to enable caching.
    /// </summary>
    /// <param name="durationMinutes">The cache duration in minutes. Default is 5 minutes.</param>
    /// <param name="cacheKeyGenerator">Optional custom cache key generator function.</param>
    protected void EnableCache(int durationMinutes = 5, Func<object, string>? cacheKeyGenerator = null)
    {
        CacheConfiguration = QueryCacheConfiguration.Enable(durationMinutes, cacheKeyGenerator);
    }

    /// <summary>
    /// Disables caching for this query handler.
    /// Call this method in the constructor of derived classes to disable caching.
    /// </summary>
    protected void DisableCache()
    {
        CacheConfiguration = QueryCacheConfiguration.Disable();
    }

    /// <summary>
    /// Sets a custom cache key generator for this query handler.
    /// </summary>
    /// <param name="cacheKeyGenerator">The function to generate cache keys from query objects.</param>
    protected void SetCacheKeyGenerator(Func<object, string> cacheKeyGenerator)
    {
        if (cacheKeyGenerator == null)
            throw new ArgumentNullException(nameof(cacheKeyGenerator));

        if (CacheConfiguration.Enabled)
        {
            CacheConfiguration = QueryCacheConfiguration.Enable(
                CacheConfiguration.DurationMinutes,
                cacheKeyGenerator);
        }
        else
        {
            // If caching is disabled, enable it with the custom key generator
            CacheConfiguration = QueryCacheConfiguration.Enable(5, cacheKeyGenerator);
        }
    }

    /// <summary>
    /// Sets the cache duration for this query handler.
    /// </summary>
    /// <param name="durationMinutes">The cache duration in minutes.</param>
    protected void SetCacheDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Cache duration must be greater than zero.", nameof(durationMinutes));

        if (CacheConfiguration.Enabled)
        {
            CacheConfiguration = QueryCacheConfiguration.Enable(
                durationMinutes,
                CacheConfiguration.CacheKeyGenerator);
        }
        else
        {
            // If caching is disabled, enable it with the specified duration
            CacheConfiguration = QueryCacheConfiguration.Enable(durationMinutes);
        }
    }

    #region Behavior Configuration Methods

    /// <summary>
    /// Enables detailed logging for this query handler.
    /// </summary>
    protected void EnableDetailedLogging()
    {
        BehaviorConfiguration.EnableDetailedLogging = true;
    }

    /// <summary>
    /// Enables performance tracking for this query handler.
    /// </summary>
    protected void EnablePerformanceTracking()
    {
        BehaviorConfiguration.EnablePerformanceTracking = true;
    }

    /// <summary>
    /// Enables retry policy for this query handler.
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
    /// Sets a timeout for query execution.
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

    #region Internal Cache Management Methods

    /// <summary>
    /// Internal method to check if caching is enabled for this handler.
    /// Used by the mediator to determine if caching should be applied.
    /// </summary>
    /// <returns>True if caching is enabled, otherwise false.</returns>
    internal bool IsCacheEnabled() => CacheConfiguration.Enabled;

    /// <summary>
    /// Internal method to get the cache duration in minutes.
    /// Used by the mediator for cache configuration.
    /// </summary>
    /// <returns>The cache duration in minutes.</returns>
    internal int GetCacheDuration() => CacheConfiguration.DurationMinutes;

    /// <summary>
    /// Internal method to get the cache key generator function.
    /// Used by the mediator for generating cache keys.
    /// </summary>
    /// <returns>The cache key generator function, or null if not set.</returns>
    internal Func<object, string>? GetCacheKeyGenerator() => CacheConfiguration.CacheKeyGenerator;

    /// <summary>
    /// Internal method to get behavior configuration.
    /// Used by the mediator for applying behaviors.
    /// </summary>
    /// <returns>The behavior configuration.</returns>
    internal HandlerBehaviorConfiguration GetBehaviorConfiguration() => BehaviorConfiguration;

    #endregion
}

