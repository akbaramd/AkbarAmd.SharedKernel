/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Mediators
 * Interface for handler behavior configuration access.
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Application.Mediators;

/// <summary>
/// Interface for accessing handler behavior configuration.
/// Allows MediatRMediator to discover behavior configurations from handlers.
/// </summary>
internal interface IHandlerBehaviorConfiguration
{
    /// <summary>
    /// Gets the behavior configuration for this handler.
    /// </summary>
    HandlerBehaviorConfiguration GetBehaviorConfiguration();
}

