/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Events
 * Base class for domain events implementing IDomainEvent with automatic Id and timestamp.
 * Year: 2025
 */

using System;

namespace CleanArchitecture.Domain.SharedKernel.Events
{
    /// <summary>
    /// Base class for domain events implementing IDomainEvent and MediatR's INotification.
    /// Automatically assigns a unique Id and OccurredOn timestamp.
    /// </summary>
    public abstract class DomainEventBase : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }

        protected DomainEventBase()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
}