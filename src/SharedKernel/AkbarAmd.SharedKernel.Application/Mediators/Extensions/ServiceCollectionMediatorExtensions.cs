/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators Extensions
 * Extension methods for registering ICqrsMediator service.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkbarAmd.SharedKernel.Application.Mediators.Extensions;

/// <summary>
/// Extension methods for registering ICqrsMediator service with dependency injection.
/// </summary>
public static class ServiceCollectionMediatorExtensions
{
    /// <summary>
    /// Adds ICqrsMediator service with MediatRMediator implementation.
    /// This registers the custom ICqrsMediator wrapper around MediatR with caching support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCqrsMediator(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register memory cache if not already registered
        services.AddMemoryCache();

        // Register ICqrsMediator with MediatRMediator implementation
        services.AddScoped<Contracts.ICqrsMediator>(serviceProvider =>
        {
            var mediator = serviceProvider.GetRequiredService<MediatR.IMediator>();
            var cache = serviceProvider.GetRequiredService<IMemoryCache>();
            var logger = serviceProvider.GetService<ILogger<MediatRMediator>>();
            
            return new MediatRMediator(mediator, cache, serviceProvider, logger);
        });

        return services;
    }
}

