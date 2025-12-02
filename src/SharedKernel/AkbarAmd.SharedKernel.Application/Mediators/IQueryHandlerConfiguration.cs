/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * Interface for query handler configuration access.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Interface for accessing query handler configuration.
/// Allows MediatRMediator to discover cache configurations from handlers.
/// </summary>
internal interface IQueryHandlerConfiguration
{
    /// <summary>
    /// Gets the cache configuration for this query handler.
    /// </summary>
    QueryCacheConfiguration GetCacheConfiguration();
}

