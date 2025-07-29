using System.Runtime.CompilerServices;
using MCA.SharedKernel.Domain.Contracts;
using MCA.SharedKernel.Domain.Events;
using MCA.SharedKernel.Domain.Exceptions;

namespace MCA.SharedKernel.Domain
{
    /// <summary>
    /// Base class for entities with a generic identity type.
    /// Provides core entity functionality including identity management,
    /// equality comparison, and business rule validation.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identity.</typeparam>
    public abstract class Entity<TId> : IEntity<TId> 
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// The entity's identifier. Protected setter allows derived classes to modify.
        /// </summary>
        public TId Id { get; protected set; }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// Validates that ID is not null or default value.
        /// </summary>
        /// <param name="id">The entity's unique identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null or default.</exception>
        protected Entity(TId id)
        {
            if (id == null || EqualityComparer<TId>.Default.Equals(id, default(TId)))
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null or default value.");
            Id = id;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected Entity()
        {
        }

        #region Equality and Comparison

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// </summary>
        /// <param name="obj">The object to compare with the current entity.</param>
        /// <returns>True if the specified object is equal to the current entity; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Entity<TId> other && Id.Equals(other.Id);
        }

        /// <summary>
        /// Determines whether the specified entity is equal to the current entity.
        /// </summary>
        /// <param name="other">The entity to compare with the current entity.</param>
        /// <returns>True if the specified entity is equal to the current entity; otherwise, false.</returns>
        public bool Equals(IEntity<TId>? other) =>
            other is not null && Id.Equals(other.Id);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current entity.</returns>
        public override int GetHashCode()
        {
            return EqualityComparer<TId>.Default.GetHashCode(Id);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Equality operator for entities.
        /// </summary>
        /// <param name="left">Left entity.</param>
        /// <param name="right">Right entity.</param>
        /// <returns>True if entities are equal; otherwise, false.</returns>
        public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator for entities.
        /// </summary>
        /// <param name="left">Left entity.</param>
        /// <param name="right">Right entity.</param>
        /// <returns>True if entities are not equal; otherwise, false.</returns>
        public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);

        #endregion

        #region Business Rule Validation

        /// <summary>
        /// Checks a business rule and throws if broken.
        /// </summary>
        /// <param name="rule">The business rule to check.</param>
        /// <param name="caller">The name of the calling method (automatically provided).</param>
        /// <exception cref="DomainBusinessRuleValidationException">Thrown when the business rule is violated.</exception>
        protected static void CheckRule(IBusinessRule rule, [CallerMemberName] string caller = null)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            if (!rule.IsSatisfied())
                throw new DomainBusinessRuleValidationException(rule);
        }

        #endregion

        #region String Representation

        /// <summary>
        /// Returns a string representation of the entity.
        /// </summary>
        /// <returns>A string that represents the current entity.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}]";

        #endregion
    }

    /// <summary>
    /// Auditable base entity class with support for soft deletes, versioning, and metadata tracking.
    /// Provides comprehensive audit trail functionality for enterprise applications.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identity.</typeparam>
    public abstract class AuditableEntity<TId> : Entity<TId> 
        where TId : IEquatable<TId>
    {
        #region Audit Properties

        /// <summary>
        /// UTC timestamp when the entity was created.
        /// </summary>
        public DateTime? CreatedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that created the entity.
        /// </summary>
        public string? CreatedBy { get; private set; }

        /// <summary>
        /// UTC timestamp when the entity was last modified.
        /// </summary>
        public DateTime? ModifiedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that last modified the entity.
        /// </summary>
        public string? ModifiedBy { get; private set; }

        /// <summary>
        /// UTC timestamp when the entity was soft deleted.
        /// </summary>
        public DateTime? DeletedAt { get; private set; }

        /// <summary>
        /// Identifier of the user/system that deleted the entity.
        /// </summary>
        public string? DeletedBy { get; private set; }

        /// <summary>
        /// Indicates whether the entity has been soft deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Optional metadata associated with the entity.
        /// </summary>
        public string? Metadata { get; private set; }

        /// <summary>
        /// Version number for optimistic concurrency control.
        /// </summary>
        public long Version { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for setting up a new auditable entity.
        /// </summary>
        /// <param name="id">The entity's unique identifier.</param>
        /// <param name="createdBy">Identifier of the creator (defaults to "system").</param>
        protected AuditableEntity(TId id, string createdBy = "system")
            : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
            IsDeleted = false;
            Version = 1;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// </summary>
        protected AuditableEntity()
        {
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
            Version = 1;
        }

        #endregion

        #region Soft Delete Operations

        /// <summary>
        /// Soft deletes the entity by setting IsDeleted to true.
        /// </summary>
        /// <param name="deletedBy">Identifier of the user/system performing the deletion.</param>
        public void Delete(string deletedBy = "system")
        {
            if (IsDeleted) return;

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
            MarkModified(deletedBy);
        }

        /// <summary>
        /// Restores a soft-deleted entity by setting IsDeleted to false.
        /// </summary>
        /// <param name="restoredBy">Identifier of the user/system performing the restoration.</param>
        public void Restore(string restoredBy = "system")
        {
            if (!IsDeleted) return;

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            MarkModified(restoredBy ?? throw new ArgumentNullException(nameof(restoredBy)));
        }

        #endregion

        #region Metadata Operations

        /// <summary>
        /// Updates the entity's metadata and marks it as modified.
        /// </summary>
        /// <param name="metadata">The new metadata value.</param>
        /// <param name="modifiedBy">Identifier of the user/system making the change.</param>
        public void UpdateMetadata(string metadata, string modifiedBy = "system")
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            MarkModified(modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy)));
        }

        #endregion

        #region Modification Tracking

        /// <summary>
        /// Marks the entity as modified, updating timestamps and version.
        /// </summary>
        /// <param name="modifiedBy">Identifier of the user/system making the modification.</param>
        public void MarkModified(string modifiedBy = "system")
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy ?? throw new ArgumentNullException(nameof(modifiedBy));
            Version++;
        }

        #endregion

        #region String Representation

        /// <summary>
        /// Returns a string representation of the auditable entity.
        /// </summary>
        /// <returns>A string that represents the current auditable entity.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}, Version={Version}, IsDeleted={IsDeleted}]";

        #endregion
    }

    #region Domain Events

    /// <summary>
    /// Domain event raised when an entity is deleted.
    /// </summary>
    /// <typeparam name="T">The type of the entity's identity.</typeparam>
    public class EntityDeletedEvent<T> : IDomainEvent
    {
        /// <summary>
        /// The identity of the deleted entity.
        /// </summary>
        public T EntityId { get; }

        /// <summary>
        /// UTC timestamp when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Unique identifier for this domain event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Constructor for creating a new entity deleted event.
        /// </summary>
        /// <param name="entityId">The identity of the deleted entity.</param>
        public EntityDeletedEvent(T entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Domain event raised when an entity is restored.
    /// </summary>
    /// <typeparam name="T">The type of the entity's identity.</typeparam>
    public class EntityRestoredEvent<T> : IDomainEvent
    {
        /// <summary>
        /// The identity of the restored entity.
        /// </summary>
        public T EntityId { get; }

        /// <summary>
        /// UTC timestamp when the event occurred.
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Unique identifier for this domain event.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Constructor for creating a new entity restored event.
        /// </summary>
        /// <param name="entityId">The identity of the restored entity.</param>
        public EntityRestoredEvent(T entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }

    #endregion
}
