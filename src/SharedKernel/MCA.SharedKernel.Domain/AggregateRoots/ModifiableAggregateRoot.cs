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
    /// Aggregate root that supports modification tracking.
    /// Inherits from CreatableAggregateRoot and implements IModifiableAggregateRoot.
    /// </summary>
    /// <typeparam name="TId">Type of the aggregate's identity.</typeparam>
    public abstract class ModifiableAggregateRoot<TId> : CreatableAggregateRoot<TId>, IModifiableAggregateRoot
        where TId : IEquatable<TId>
    {
        #region IModifiableAggregateRoot Implementation

        /// <summary>
        /// UTC timestamp when the aggregate was last modified.
        /// </summary>
        public DateTime? ModifiedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that last modified the aggregate.
        /// </summary>
        public string? ModifiedBy { get; private set; }

        /// <summary>
        /// UTC timestamp of last modification (UTC).
        /// </summary>
        public DateTimeOffset LastModifiedUtc { get; private set; }

        /// <summary>
        /// Marks the aggregate as modified, updating timestamps.
        /// </summary>
        /// <param name="modifiedBy">Identifier of the user/system making the modification.</param>
        public void MarkModified(string modifiedBy)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
            LastModifiedUtc = DateTimeOffset.UtcNow;
            IncrementVersion();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new modifiable aggregate root instance.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        protected ModifiableAggregateRoot(TId id) : base(id)
        {
            LastModifiedUtc = CreatedAtUtc;
        }

        /// <summary>
        /// Creates a new modifiable aggregate root instance with creator.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        /// <param name="createdBy">Identifier of the creator.</param>
        protected ModifiableAggregateRoot(TId id, string createdBy) : base(id, createdBy)
        {
            LastModifiedUtc = CreatedAtUtc;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// </summary>
        protected ModifiableAggregateRoot() : base()
        {
            LastModifiedUtc = CreatedAtUtc;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// String representation for debugging and logging.
        /// </summary>
        /// <returns>A string that represents the current modifiable aggregate root.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}, Version={Version}, CreatedBy={CreatedBy}, ModifiedBy={ModifiedBy}]";

        #endregion
    }
}
