/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Events
 * IDomainEvent interface representing a domain event with MediatR integration.
 * Year: 2025
 */

using MediatR;
using System;

namespace CleanArchitecture.Domain.SharedKernel.Events
{
    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
    }
}