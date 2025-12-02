/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * Handler behavior configuration for advanced features.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Configuration for handler behaviors including logging, performance tracking, retry policies, etc.
/// </summary>
public sealed class HandlerBehaviorConfiguration
{
    /// <summary>
    /// Gets or sets whether detailed logging is enabled.
    /// </summary>
    public bool EnableDetailedLogging { get; set; }

    /// <summary>
    /// Gets or sets whether performance tracking is enabled.
    /// </summary>
    public bool EnablePerformanceTracking { get; set; }

    /// <summary>
    /// Gets or sets whether retry policy is enabled.
    /// </summary>
    public bool EnableRetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retry attempts in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether validation is enabled before processing.
    /// </summary>
    public bool EnableValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether transaction management is enabled.
    /// </summary>
    public bool EnableTransaction { get; set; }

    /// <summary>
    /// Gets or sets the timeout in seconds for the operation.
    /// </summary>
    public int? TimeoutSeconds { get; set; }

    /// <summary>
    /// Creates a default behavior configuration.
    /// </summary>
    /// <returns>A HandlerBehaviorConfiguration instance with default settings.</returns>
    public static HandlerBehaviorConfiguration Default()
    {
        return new HandlerBehaviorConfiguration
        {
            EnableDetailedLogging = false,
            EnablePerformanceTracking = false,
            EnableRetryPolicy = false,
            EnableValidation = true,
            EnableTransaction = false
        };
    }

    /// <summary>
    /// Creates a behavior configuration with all features enabled.
    /// </summary>
    /// <returns>A HandlerBehaviorConfiguration instance with all features enabled.</returns>
    public static HandlerBehaviorConfiguration Full()
    {
        return new HandlerBehaviorConfiguration
        {
            EnableDetailedLogging = true,
            EnablePerformanceTracking = true,
            EnableRetryPolicy = true,
            MaxRetryAttempts = 3,
            RetryDelayMs = 1000,
            EnableValidation = true,
            EnableTransaction = true
        };
    }

    /// <summary>
    /// Creates a behavior configuration with performance tracking enabled.
    /// </summary>
    /// <returns>A HandlerBehaviorConfiguration instance with performance tracking enabled.</returns>
    public static HandlerBehaviorConfiguration WithPerformanceTracking()
    {
        return new HandlerBehaviorConfiguration
        {
            EnablePerformanceTracking = true,
            EnableDetailedLogging = true
        };
    }

    /// <summary>
    /// Creates a behavior configuration with retry policy enabled.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of retry attempts.</param>
    /// <param name="delayMs">Delay between retries in milliseconds.</param>
    /// <returns>A HandlerBehaviorConfiguration instance with retry policy enabled.</returns>
    public static HandlerBehaviorConfiguration WithRetry(int maxAttempts = 3, int delayMs = 1000)
    {
        return new HandlerBehaviorConfiguration
        {
            EnableRetryPolicy = true,
            MaxRetryAttempts = maxAttempts,
            RetryDelayMs = delayMs
        };
    }
}

