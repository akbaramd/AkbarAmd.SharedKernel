namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support versioning and concurrency control.
/// </summary>
public interface IVersionableAggregateRoot
{
    /// <summary>
    /// Current version number for optimistic concurrency control.
    /// </summary>
    long Version { get; }

    /// <summary>
    /// Optional snapshot version, useful for event sourcing or caching.
    /// </summary>
    long SnapshotVersion { get; }

    /// <summary>
    /// Increment version after successful commit.
    /// </summary>
    void IncrementVersion();

    /// <summary>
    /// Create a snapshot representing current state.
    /// </summary>
    /// <returns>Snapshot object or null if not implemented.</returns>
    object CreateSnapshot();

    /// <summary>
    /// Restore aggregate state from snapshot.
    /// </summary>
    /// <param name="snapshot">Snapshot object to restore from.</param>
    void RestoreFromSnapshot(object snapshot);

    /// <summary>
    /// Called after rehydrating aggregate from events or snapshot.
    /// </summary>
    void OnRehydrated();
}