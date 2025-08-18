/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Base Types
 * AggregateRoot base class supporting identity, domain events, versioning, snapshotting, and concurrency.
 * Year: 2025
 */

using MCA.SharedKernel.Domain.Contracts;

namespace MCA.SharedKernel.Domain
{
    /// <summary>
    /// Comprehensive aggregate root that supports all audit capabilities.
    /// Inherits from DeletableAggregateRoot and implements IFullyAuditableAggregateRoot.
    /// This is the most feature-rich aggregate root class.
    /// </summary>
    /// <typeparam name="TId">Type of the aggregate's identity.</typeparam>
    public abstract class FullyAuditableAggregateRoot<TId> : DeletableAggregateRoot<TId>, IFullyAuditableAggregateRoot
        where TId : IEquatable<TId>
    {
        #region IVersionableAggregateRoot Implementation

        /// <summary>
        /// Optional snapshot version, useful for event sourcing or caching.
        /// </summary>
        public long SnapshotVersion { get; protected set; }

        /// <summary>
        /// Create a snapshot representing current state.
        /// Override in derived classes for event-sourced aggregates.
        /// </summary>
        /// <returns>Snapshot object or null if not implemented.</returns>
        public virtual object CreateSnapshot() => null;

        /// <summary>
        /// Restore aggregate state from snapshot.
        /// Override in derived classes for event-sourced aggregates.
        /// </summary>
        /// <param name="snapshot">Snapshot object to restore from.</param>
        public virtual void RestoreFromSnapshot(object snapshot) { }

        /// <summary>
        /// Called after rehydrating aggregate from events or snapshot.
        /// Resets transient state and clears domain events.
        /// </summary>
        public virtual void OnRehydrated()
        {
            ClearDomainEvents();
        }

        #endregion

        #region IMetadataAggregateRoot Implementation

        /// <summary>
        /// Optional metadata associated with the aggregate.
        /// </summary>
        public string? Metadata { get; private set; }

        /// <summary>
        /// Updates the aggregate's metadata and marks it as modified.
        /// </summary>
        /// <param name="metadata">The new metadata value.</param>
        /// <param name="modifiedBy">Identifier of the user/system making the change.</param>
        public void UpdateMetadata(string metadata, string modifiedBy)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            MarkModified(modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy)));
        }

        #endregion

        #region IFullyAuditableAggregateRoot Additional Implementation

        /// <summary>
        /// UTC timestamp when the aggregate was last accessed (read).
        /// </summary>
        public DateTime? LastAccessedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that last accessed the aggregate.
        /// </summary>
        public string? LastAccessedBy { get; private set; }

        /// <summary>
        /// Number of times the aggregate has been accessed.
        /// </summary>
        public long AccessCount { get; private set; }

        /// <summary>
        /// UTC timestamp when the aggregate was last backed up.
        /// </summary>
        public DateTime? LastBackupAt { get; private set; }

        /// <summary>
        /// UTC timestamp when the aggregate was last archived.
        /// </summary>
        public DateTime? LastArchivedAt { get; private set; }

        /// <summary>
        /// Marks the aggregate as accessed.
        /// </summary>
        /// <param name="accessedBy">Identifier of the user/system accessing the aggregate.</param>
        public virtual void MarkAccessed(string accessedBy)
        {
            LastAccessedAt = DateTime.UtcNow;
            LastAccessedBy = accessedBy ?? throw new ArgumentNullException(nameof(accessedBy));
            AccessCount++;
        }

        /// <summary>
        /// Marks the aggregate as backed up.
        /// </summary>
        public virtual void MarkBackedUp()
        {
            LastBackupAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the aggregate as archived.
        /// </summary>
        public virtual void MarkArchived()
        {
            LastArchivedAt = DateTime.UtcNow;
            MarkModified("system");
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new fully auditable aggregate root instance.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        protected FullyAuditableAggregateRoot(TId id) : base(id)
        {
            AccessCount = 0;
        }

        /// <summary>
        /// Creates a new fully auditable aggregate root instance with creator.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        /// <param name="createdBy">Identifier of the creator.</param>
        protected FullyAuditableAggregateRoot(TId id, string createdBy) : base(id, createdBy)
        {
            AccessCount = 0;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// </summary>
        protected FullyAuditableAggregateRoot() : base()
        {
            AccessCount = 0;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// String representation for debugging and logging.
        /// </summary>
        /// <returns>A string that represents the current fully auditable aggregate root.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}, Version={Version}, IsDeleted={IsDeleted}, AccessCount={AccessCount}]";

        #endregion
    }
}
