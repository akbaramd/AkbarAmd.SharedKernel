using System;
using MediatR;

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
