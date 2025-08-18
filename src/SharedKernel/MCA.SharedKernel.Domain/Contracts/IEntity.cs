/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork
 * Contract for Domain Entities.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Base contract for all domain entities without generic constraints.
    /// Provides common entity functionality for key management and comparison.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets the primary keys that identify this entity.
        /// </summary>
        /// <returns>Array of objects representing the entity's primary keys.</returns>
        object[] GetKeys();
    }

    /// <summary>
    /// Contract for domain entities with a strongly-typed identity.
    /// Guarantees identity access and comparison without exposing implementation details.
    /// </summary>
    /// <typeparam name="TId">Identifier type (value object or primitive) implementing <see cref="IEquatable{T}"/>.</typeparam>
    public interface IEntity<TId> : IEntity, IEquatable<IEntity<TId>>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Unique identifier of the entity.
        /// </summary>
        TId Id { get; }

        /// <summary>
        /// Indicates whether the entity has not been persisted yet (no stable identity assigned).
        /// </summary>
        bool IsTransient => Id is null || Id.Equals(default);
        
        /// <summary>
        /// Checks identity equality independent of runtime type.
        /// </summary>
        /// <param name="other">Another entity.</param>
        /// <returns>True if both entities share the same identity.</returns>
        bool SameIdentityAs(IEntity<TId> other) => other is not null && Id.Equals(other.Id);
    }
}