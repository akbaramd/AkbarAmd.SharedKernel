/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Events
 * IDomainEvent interface representing a domain event with MediatR integration.
 * Year: 2025
 */

using MediatR;

namespace MCA.SharedKernel.Domain.Contracts
{
    public interface IDomainEvent : INotification
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
    }
}