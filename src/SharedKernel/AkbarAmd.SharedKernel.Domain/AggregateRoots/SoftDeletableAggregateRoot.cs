/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Soft Deletable Aggregate Root
 * Aggregate root with creation, modification, and soft delete tracking
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Domain.AggregateRoots
{
    /// <summary>
    /// Aggregate root class with creation, modification, and soft delete tracking capabilities
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier</typeparam>
    public abstract class SoftDeletableAggregateRoot<TKey> : ModifiableAggregateRoot<TKey>, ISoftDeletableAudit
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Whether the entity is soft deleted (required)
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// When the entity was deleted (nullable)
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Who deleted the entity (nullable)
        /// </summary>
        public string? DeletedBy { get; set; }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected SoftDeletableAggregateRoot()
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        protected SoftDeletableAggregateRoot(TKey id) : base(id)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID and creation tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        protected SoftDeletableAggregateRoot(TKey id, string createdBy) : base(id, createdBy)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID, creation, and modification tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        /// <param name="modifiedBy">Who modified the entity</param>
        protected SoftDeletableAggregateRoot(TKey id, string createdBy, string modifiedBy) : base(id, createdBy, modifiedBy)
        {
        }

        /// <summary>
        /// Marks the entity as soft deleted by the specified user.
        /// </summary>
        /// <param name="deletedBy">Who deleted the entity</param>
        /// <exception cref="ArgumentNullException">Thrown when deletedBy is null</exception>
        public void MarkDeleted(string deletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
        }

        /// <summary>
        /// Marks the entity as restored (undeleted).
        /// </summary>
        public void MarkRestored()
        {
            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
        }
    }
}