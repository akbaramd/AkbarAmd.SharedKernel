/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Creatable Aggregate Root
 * Aggregate root with creation tracking only
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts.Audits;

namespace AkbarAmd.SharedKernel.Domain.AggregateRoots
{
    /// <summary>
    /// Aggregate root class with creation tracking capabilities
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier</typeparam>
    public abstract class CreatableAggregateRoot<TKey> : AggregateRoot<TKey>, ICreatableAudit
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// When the entity was created (required)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Who created the entity (required)
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected CreatableAggregateRoot()
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        protected CreatableAggregateRoot(TKey id) : base(id)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID and creation tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        protected CreatableAggregateRoot(TKey id, string createdBy) : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        }

        /// <summary>
        /// Marks the entity as created by the specified user.
        /// </summary>
        /// <param name="createdBy">Who created the entity</param>
        public void MarkCreated(string createdBy)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        }
    }
}