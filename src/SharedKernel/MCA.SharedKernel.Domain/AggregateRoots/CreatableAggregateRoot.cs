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
    /// Aggregate root that supports creation tracking.
    /// Implements ICreatableAggregateRoot for basic creation audit.
    /// </summary>
    /// <typeparam name="TId">Type of the aggregate's identity.</typeparam>
    public abstract class CreatableAggregateRoot<TId> : AggregateRoot<TId>, ICreatableAggregateRoot
        where TId : IEquatable<TId>
    {
        #region ICreatableAggregateRoot Implementation

        /// <summary>
        /// UTC timestamp when the aggregate was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that created the aggregate.
        /// </summary>
        public string CreatedBy { get; private set; }

        /// <summary>
        /// UTC timestamp when the aggregate was created (UTC).
        /// </summary>
        public DateTimeOffset CreatedAtUtc { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new creatable aggregate root instance.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        protected CreatableAggregateRoot(TId id) : base(id)
        {
            var now = DateTime.UtcNow;
            var nowUtc = DateTimeOffset.UtcNow;
            
            CreatedAt = now;
            CreatedBy = "system";
            CreatedAtUtc = nowUtc;
        }

        /// <summary>
        /// Creates a new creatable aggregate root instance with creator.
        /// </summary>
        /// <param name="id">The aggregate's unique identifier.</param>
        /// <param name="createdBy">Identifier of the creator.</param>
        protected CreatableAggregateRoot(TId id, string createdBy) : base(id, createdBy)
        {
            var now = DateTime.UtcNow;
            var nowUtc = DateTimeOffset.UtcNow;
            
            CreatedAt = now;
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
            CreatedAtUtc = nowUtc;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// </summary>
        protected CreatableAggregateRoot() : base()
        {
            var now = DateTime.UtcNow;
            var nowUtc = DateTimeOffset.UtcNow;
            
            CreatedAt = now;
            CreatedBy = "system";
            CreatedAtUtc = nowUtc;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// String representation for debugging and logging.
        /// </summary>
        /// <returns>A string that represents the current creatable aggregate root.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}, Version={Version}, CreatedBy={CreatedBy}]";

        #endregion
    }
}
