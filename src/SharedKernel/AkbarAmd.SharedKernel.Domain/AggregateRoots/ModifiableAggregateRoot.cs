/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Modifiable Aggregate Root
 * Aggregate root with creation and modification tracking
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Domain.AggregateRoots
{
    /// <summary>
    /// Aggregate root class with creation and modification tracking capabilities
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier</typeparam>
    public abstract class ModifiableAggregateRoot<TKey> : CreatableAggregateRoot<TKey>, IModifiableAudit
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// When the entity was last modified (nullable)
        /// </summary>
        public DateTime? ModifiedAt { get; set; }

        /// <summary>
        /// Who last modified the entity (nullable)
        /// </summary>
        public string? ModifiedBy { get; set; }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected ModifiableAggregateRoot()
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        protected ModifiableAggregateRoot(TKey id) : base(id)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID and creation tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        protected ModifiableAggregateRoot(TKey id, string createdBy) : base(id, createdBy)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID, creation, and modification tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        /// <param name="modifiedBy">Who modified the entity</param>
        protected ModifiableAggregateRoot(TKey id, string createdBy, string modifiedBy) : base(id, createdBy)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        }

        /// <summary>
        /// Marks the entity as modified by the specified user.
        /// </summary>
        /// <param name="modifiedBy">Who modified the entity</param>
        /// <exception cref="ArgumentNullException">Thrown when modifiedBy is null</exception>
        public void MarkModified(string modifiedBy)
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
        }
    }
}