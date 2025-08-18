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
    /// Aggregate root that supports soft delete functionality.
    /// Inherits from ModifiableAggregateRoot and implements IDeletableAggregateRoot.
    /// </summary>
    /// <typeparam name="TId">Type of the aggregate's identity.</typeparam>
    public abstract class DeletableAggregateRoot<TId> : ModifiableAggregateRoot<TId>, IDeletableAggregateRoot
        where TId : IEquatable<TId>
    {
        #region IDeletableAggregateRoot Implementation

        /// <summary>
        /// UTC timestamp when the aggregate was soft deleted.
        /// </summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that deleted the aggregate.
        /// </summary>
        public string? DeletedBy { get; private set; }

        /// <summary>
        /// Indicates whether the aggregate has been soft deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Soft deletes the aggregate by setting IsDeleted to true.
        /// </summary>
        /// <param name="deletedBy">Identifier of the user/system performing the deletion.</param>
        public void Delete(string deletedBy)
        {
            if (IsDeleted) return;

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
            MarkModified(deletedBy);
        }

        /// <summary>
        /// Restores a soft-deleted aggregate by setting IsDeleted to false.
        /// </summary>
        /// <param name="restoredBy">Identifier of the user/system performing the restoration.</param>
        public void Restore(string restoredBy)
        {
            if (!IsDeleted) return;

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            MarkModified(restoredBy ?? throw new ArgumentNullException(nameof(restoredBy)));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new deletable aggregate root instance.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        protected DeletableAggregateRoot(TId id) : base(id)
        {
            IsDeleted = false;
        }

        /// <summary>
        /// Creates a new deletable aggregate root instance with creator.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        /// <param name="createdBy">Identifier of the creator.</param>
        protected DeletableAggregateRoot(TId id, string createdBy) : base(id, createdBy)
        {
            IsDeleted = false;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// </summary>
        protected DeletableAggregateRoot() : base()
        {
            IsDeleted = false;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// String representation for debugging and logging.
        /// </summary>
        /// <returns>A string that represents the current deletable aggregate root.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}, Version={Version}, IsDeleted={IsDeleted}]";

        #endregion
    }
}
