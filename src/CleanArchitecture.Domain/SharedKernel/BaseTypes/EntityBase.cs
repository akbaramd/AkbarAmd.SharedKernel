using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes
{
    /// <summary>
    /// Base class for entities with a generic identity type.
    /// </summary>
    public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>> where TId : IEquatable<TId>
    {
        // The entity's identifier (ensure it's never null)
        public TId Id { get; protected set; }

        // Constructor for setting up entity with ID (ID should always have a value)
        protected EntityBase(TId id)
        {
            if (id == null || EqualityComparer<TId>.Default.Equals(id, default(TId)))
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null or default.");
            Id = id;
        }

        // Default constructor for ORM/ODM support
        protected EntityBase()
        {
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals(obj as EntityBase<TId>);
        }

        public bool Equals(EntityBase<TId>? other)
        {
            if (other is null) return false;

            // Ensure that Id is not null or default value
            return !EqualityComparer<TId>.Default.Equals(Id, default(TId)) && Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            // Ensure a valid Id is set, otherwise return default
            return EqualityComparer<TId>.Default.Equals(Id, default(TId)) ? 0 : Id.GetHashCode();
        }

        public static bool operator ==(EntityBase<TId>? left, EntityBase<TId>? right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(EntityBase<TId>? left, EntityBase<TId>? right) => !(left == right);
    }

    /// <summary>
    /// Auditable base entity class with support for soft deletes, versioning, and metadata tracking.
    /// </summary>
    public abstract class AuditableEntityBase<TId> : EntityBase<TId> where TId : IEquatable<TId>
    {
        // Entity audit properties
        public DateTime? CreatedAt { get; private set; }
        public string? CreatedBy { get; private set; }
        public DateTime? ModifiedAt { get; private set; }
        public string? ModifiedBy { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }
        public bool IsDeleted { get; private set; }
        public string? Metadata { get; private set; }
        public long Version { get; private set; }  // Version for concurrency control

        // Constructor for setting up a new entity (with auditing)
        protected AuditableEntityBase(TId id, string createdBy = "system")
            : base(id)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy;
            IsDeleted = false;
            Version = 1;  // Initial versioning setup
        }

        // Default constructor for ORM/ODM support
        protected AuditableEntityBase()
        {
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
            Version = 1;
        }

        // Soft delete functionality
        public void Delete(string deletedBy = "system")
        {
            if (IsDeleted) return;

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            MarkModified(deletedBy);
        }

        // Restore functionality for soft-deleted entities
        public void Restore(string restoredBy = "system")
        {
            if (!IsDeleted) return;

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            MarkModified(restoredBy);
        }

        // Update metadata and modify timestamp
        public void UpdateMetadata(string metadata, string modifiedBy = "system")
        {
            Metadata = metadata;
            MarkModified(modifiedBy);
        }

        // Mark the entity as modified (updating ModifiedAt and incrementing version)
        public void MarkModified(string modifiedBy = "system")
        {
            ModifiedAt = DateTime.UtcNow;
            ModifiedBy = modifiedBy;
            Version++;  // Increment version for concurrency control
        }
    }

    // Event raised when an entity is deleted
    public class EntityDeletedEvent<T> : IDomainEvent
    {
        public T EntityId { get; }
        public DateTime OccurredOn { get; }
        public Guid Id { get; }

        public EntityDeletedEvent(T entityId)
        {
            Id = Guid.NewGuid();
            EntityId = entityId;
            OccurredOn = DateTime.UtcNow;
        }
    }

    // Event raised when an entity is restored
    public class EntityRestoredEvent<T> : IDomainEvent
    {
        public T EntityId { get; }
        public DateTime OccurredOn { get; }
        public Guid Id { get; }

        public EntityRestoredEvent(T entityId)
        {
            Id = Guid.NewGuid();
            EntityId = entityId;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
