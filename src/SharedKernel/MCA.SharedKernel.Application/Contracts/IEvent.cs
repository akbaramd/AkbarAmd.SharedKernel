/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Application Shared Kernel - Events
 * IEvent interface for integration events and application-level events.
 * Year: 2025
 */

using MediatR;

namespace MCA.SharedKernel.Application.Contracts;

/// <summary>
/// Marker interface for integration events and application-level events.
/// Used for cross-bounded context communication and external system integration.
/// Integration events are different from domain events as they represent
/// business operations that have been completed and are ready for external consumption.
/// </summary>
public interface IEvent : INotification
{
    /// <summary>
    /// Unique identifier for the integration event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// UTC timestamp when the integration event occurred.
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Correlation ID for tracking the event across different systems.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Source system or bounded context that published this event.
    /// </summary>
    string Source { get; }
}