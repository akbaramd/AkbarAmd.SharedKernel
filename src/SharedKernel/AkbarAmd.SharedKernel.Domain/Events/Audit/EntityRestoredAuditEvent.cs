/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Entity Restored Audit Event
 * Domain event for entity restoration audit tracking
 * Year: 2025
 */

using AkbarAmd.SharedKernel.Domain.Contracts;

namespace AkbarAmd.SharedKernel.Domain.Events.Audit
{
    /// <summary>
    /// Domain event raised when an entity is restored (undeleted) for audit purposes
    /// </summary>
    public class EntityRestoredAuditEvent : IDomainEvent
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
        /// Entity identifier that was restored
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// Who restored the entity
        /// </summary>
        public string RestoredBy { get; }

        /// <summary>
        /// When the entity was restored
        /// </summary>
        public DateTime RestoredAt { get; }

        /// <summary>
        /// Creates a new entity restored audit event
        /// </summary>
        /// <param name="entityId">Entity identifier that was restored</param>
        /// <param name="restoredBy">Who restored the entity</param>
        /// <param name="restoredAt">When the entity was restored</param>
        public EntityRestoredAuditEvent(string entityId, string restoredBy, DateTime restoredAt)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            RestoredBy = restoredBy ?? throw new ArgumentNullException(nameof(restoredBy));
            RestoredAt = restoredAt;
            OccurredOn = DateTime.UtcNow;
        }
    }
}