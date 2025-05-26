/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain SeedWork
 * Interface for Aggregate Root with domain events, versioning, and snapshot support.
 * Year: 2025
 */

using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.SharedKernel.Interfaces
{
    /// <summary>
    /// Contract for Aggregate Roots in Domain-Driven Design.
    /// Supports domain events, versioning, snapshotting, and state management.
    /// </summary>
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the current version of the aggregate for concurrency control.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Gets the optional snapshot version of the aggregate.
        /// </summary>
        long SnapshotVersion { get; }

        /// <summary>
        /// Gets the UTC timestamp when the aggregate was last modified.
        /// </summary>
        DateTimeOffset LastModifiedUtc { get; }

        /// <summary>
        /// Indicates if the aggregate has any pending domain events to be dispatched.
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
        /// Creates a snapshot representing the current state of the aggregate.
        /// </summary>
        /// <returns>An object representing the snapshot.</returns>
        object CreateSnapshot();

        /// <summary>
        /// Restores the aggregate's state from the given snapshot.
        /// </summary>
        /// <param name="snapshot">Snapshot object.</param>
        void RestoreFromSnapshot(object snapshot);

        /// <summary>
        /// Method called after rehydrating the aggregate to reset transient state.
        /// </summary>
        void OnRehydrated();

        /// <summary>
        /// Increments the aggregate's version after a successful commit.
        /// </summary>
        void IncrementVersion();
    }
}
