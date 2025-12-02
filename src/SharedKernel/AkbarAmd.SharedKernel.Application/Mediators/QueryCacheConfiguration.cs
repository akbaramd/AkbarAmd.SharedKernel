/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * Query cache configuration for query handlers.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Configuration for query result caching.
/// Used to configure caching behavior in query handlers without using attributes.
/// </summary>
public sealed class QueryCacheConfiguration
{
    /// <summary>
    /// Gets or sets whether caching is enabled for this query handler.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// Default is 5 minutes.
    /// </summary>
    public int DurationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets a custom cache key generator function.
    /// If not provided, a default key will be generated based on query type and hash.
    /// </summary>
    public Func<object, string>? CacheKeyGenerator { get; set; }

    /// <summary>
    /// Creates a cache configuration with caching enabled.
    /// </summary>
    /// <param name="durationMinutes">The cache duration in minutes.</param>
    /// <param name="cacheKeyGenerator">Optional custom cache key generator.</param>
    /// <returns>A configured QueryCacheConfiguration instance.</returns>
    public static QueryCacheConfiguration Enable(int durationMinutes = 5, Func<object, string>? cacheKeyGenerator = null)
    {
        return new QueryCacheConfiguration
        {
            Enabled = true,
            DurationMinutes = durationMinutes,
            CacheKeyGenerator = cacheKeyGenerator
        };
    }

    /// <summary>
    /// Creates a cache configuration with caching disabled.
    /// </summary>
    /// <returns>A QueryCacheConfiguration instance with caching disabled.</returns>
    public static QueryCacheConfiguration Disable()
    {
        return new QueryCacheConfiguration
        {
            Enabled = false
        };
    }
}

