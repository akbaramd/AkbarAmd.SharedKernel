/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Entity Soft Deleted Audit Event
 * Domain event for entity soft deletion audit tracking
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts;

namespace AkbarAmd.SharedKernel.Domain.Events.Audit
{
    /// <summary>
    /// Domain event raised when an entity is soft deleted for audit purposes
    /// </summary>
    public class EntitySoftDeletedAuditEvent : IDomainEvent
    {
        /// <summary>
        /// Unique identifier for this domain event
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Entity identifier that was deleted
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// Who deleted the entity
        /// </summary>
        public string DeletedBy { get; }

        /// <summary>
        /// When the entity was deleted
        /// </summary>
        public DateTime DeletedAt { get; }

        /// <summary>
        /// Creates a new entity soft deleted audit event
        /// </summary>
        /// <param name="entityId">Entity identifier that was deleted</param>
        /// <param name="deletedBy">Who deleted the entity</param>
        /// <param name="deletedAt">When the entity was deleted</param>
        public EntitySoftDeletedAuditEvent(string entityId, string deletedBy, DateTime deletedAt)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
            DeletedAt = deletedAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}