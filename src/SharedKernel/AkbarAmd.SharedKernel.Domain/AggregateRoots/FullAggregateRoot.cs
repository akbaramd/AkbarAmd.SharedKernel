/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Full Aggregate Root
 * Aggregate root with all tracking capabilities (creation, modification, soft delete, concurrency)
 * Year: 2025
 */

namespace AkbarAmd.SharedKernel.Domain.AggregateRoots
{
    /// <summary>
    /// Full aggregate root class with all tracking capabilities (creation, modification, soft delete, and concurrency)
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier</typeparam>
    public abstract class FullAggregateRoot<TKey> : SoftDeletableAggregateRoot<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected FullAggregateRoot()
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        protected FullAggregateRoot(TKey id) : base(id)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID and creation tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        protected FullAggregateRoot(TKey id, string createdBy) : base(id, createdBy)
        {
        }

        /// <summary>
        /// Constructor for setting up entity with ID, creation, and modification tracking.
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="createdBy">Who created the entity</param>
        /// <param name="modifiedBy">Who modified the entity</param>
        protected FullAggregateRoot(TKey id, string createdBy, string modifiedBy) : base(id, createdBy, modifiedBy)
        {
        }
    }
}