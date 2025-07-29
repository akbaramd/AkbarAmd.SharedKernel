/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork
 * Interface for Aggregate Root with domain events, versioning, and snapshot support.
 * Year: 2025
 */

using MCA.SharedKernel.Domain.Events;

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Contract for Aggregate Roots in Domain-Driven Design.
    /// Supports domain events, versioning, snapshotting, and state management.
    /// This interface is designed to be globally applicable across all bounded contexts.
    /// </summary>
    /// <typeparam name="TKey">The type of the aggregate's identity key.</typeparam>
    public interface IAggregateRoot<TKey> : IEntity<TKey> 
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the current version of the aggregate for optimistic concurrency control.
        /// This version is incremented on each successful state change.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the optional snapshot version of the aggregate.
        /// Useful for event sourcing and caching strategies.
        /// </summary>
        long SnapshotVersion { get; }

        /// <summary>
        /// Gets the UTC timestamp when the aggregate was last modified.
        /// Used for audit trails and change tracking.
        /// </summary>
        DateTimeOffset LastModifiedUtc { get; }

        /// <summary>
        /// Indicates if the aggregate has any pending domain events to be dispatched.
        /// Thread-safe property for checking event state.
        /// </summary>
        bool HasPendingEvents { get; }

        /// <summary>
        /// Read-only collection of pending domain events.
        /// Thread-safe snapshot of current events.
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Clears all pending domain events.
        /// Called after successful event dispatch to prevent duplicate processing.
        /// </summary>
        void ClearDomainEvents();

        /// <summary>
        /// Creates a snapshot representing the current state of the aggregate.
        /// Override in derived classes to provide specific snapshot implementations.
        /// </summary>
        /// <returns>An object representing the snapshot state.</returns>
        object CreateSnapshot();

        /// <summary>
        /// Restores the aggregate's state from the given snapshot.
        /// Override in derived classes to handle specific snapshot types.
        /// </summary>
        /// <param name="snapshot">Snapshot object to restore from.</param>
        /// <exception cref="ArgumentException">Thrown when snapshot is invalid or incompatible.</exception>
        void RestoreFromSnapshot(object snapshot);

        /// <summary>
        /// Method called after rehydrating the aggregate to reset transient state.
        /// Clears domain events and resets any temporary state.
        /// </summary>
        void OnRehydrated();

        /// <summary>
        /// Increments the aggregate's version after a successful commit.
        /// Thread-safe operation for concurrency control.
        /// </summary>
        void IncrementVersion();
    }

    /// <summary>
    /// Non-generic marker interface for aggregate roots.
    /// Useful for type checking and dependency injection scenarios.
    /// </summary>
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the current version of the aggregate.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the UTC timestamp when the aggregate was last modified.
        /// </summary>
        DateTimeOffset LastModifiedUtc { get; }

        /// <summary>
        /// Indicates if the aggregate has any pending domain events.
        /// </summary>
        bool HasPendingEvents { get; }

        /// <summary>
        /// Read-only collection of pending domain events.
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Clears all pending domain events.
        /// </summary>
        void ClearDomainEvents();

        /// <summary>
        /// Increments the aggregate's version.
        /// </summary>
        void IncrementVersion();
    }
}
