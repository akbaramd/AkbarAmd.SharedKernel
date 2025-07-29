/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * QueryHandler base class for query processing with DDD alignment.
 * Year: 2025
 */

using MCA.SharedKernel.Application.Contracts;
using MediatR;

namespace MCA.SharedKernel.Application.Mediators;

/// <summary>
/// Base class for handling queries that retrieve data without modifying system state.
/// Queries represent read operations and should be handled within the application layer
/// following CQRS principles. Queries should be idempotent and not cause side effects.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public abstract class QueryHandler<TQuery, TResult> : IRequestHandler<TQuery, TResult> 
    where TQuery : IQuery<TResult>
{
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

            // Check cache (if applicable)
            var cachedResult = await TryGetFromCache(request);
            if (cachedResult != null)
            {
                await OnQueryProcessedFromCache(request, cachedResult);
                return cachedResult;
            }

            // Process the query using protected method
            var result = await ProcessAsync(request, cancellationToken);

            // Cache result (if applicable)
            await CacheResult(request, result);

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
    /// Override to implement custom cache key generation.
    /// </summary>
    /// <param name="request">The query to generate a cache key for.</param>
    /// <returns>A cache key string.</returns>
    protected virtual string GetCacheKey(TQuery request)
    {
        // Default implementation - use query type and hash
        return $"{typeof(TQuery).Name}_{request.GetHashCode()}";
    }

    /// <summary>
    /// Determines if the query result should be cached.
    /// Override to implement custom caching logic.
    /// </summary>
    /// <param name="request">The query to check.</param>
    /// <returns>True if the result should be cached, otherwise false.</returns>
    protected virtual bool ShouldCache(TQuery request)
    {
        // Default implementation - no caching
        return false;
    }

    /// <summary>
    /// Gets the cache duration for the query result.
    /// Override to implement custom cache duration logic.
    /// </summary>
    /// <param name="request">The query to get cache duration for.</param>
    /// <returns>The cache duration in minutes.</returns>
    protected virtual int GetCacheDurationMinutes(TQuery request)
    {
        // Default implementation - 5 minutes
        return 5;
    }
}

