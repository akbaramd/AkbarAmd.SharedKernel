/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Events
 * Base class for domain events implementing IDomainEvent with automatic Id and timestamp.
 * Comprehensive auditable event system for tracking all domain changes.
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts;

namespace AkbarAmd.SharedKernel.Domain.Events
{
    #region Base Domain Event

    /// <summary>
    /// Base class for domain events implementing IDomainEvent and MediatR's INotification.
    /// Automatically assigns a unique Id and OccurredOn timestamp.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Unique identifier for this domain event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// UTC timestamp when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Constructor for creating a new domain event.
        /// Automatically assigns Id and OccurredOn.
        /// </summary>
        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }

    #endregion

    #region Auditable Domain Events

    #endregion

    #region Entity Lifecycle Events

    #endregion

    #region Aggregate Root Events

    #endregion

    #region Access and Usage Events

    #endregion

    #region Metadata and Versioning Events

    #endregion

    #region Snapshot and Event Sourcing Events

    #endregion

    #region Backup and Archive Events

    #endregion
}