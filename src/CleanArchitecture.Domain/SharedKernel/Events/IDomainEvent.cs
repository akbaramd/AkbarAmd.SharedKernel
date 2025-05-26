using System;
using MediatR;

namespace CleanArchitecture.Domain.SharedKernel.Events;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
} 