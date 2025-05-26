/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Base Types
 * EntityBase class with generic identity, equality, versioning, and auditing support.
 * Year: 2025
 */

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes
{
    /// <summary>
    /// Base class for entities with generic identity type.
    /// Supports equality, hashing, versioning, auditing, and validation.
    /// </summary>
    /// <typeparam name="TId">Type of the entity's identifier.</typeparam>
    public abstract class EntityBase<TId> : IEquatable<EntityBase<TId>>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Gets the unique identifier for this entity.
        /// Immutable after construction.
        /// </summary>
        public TId Id { get; }

        /// <summary>
        /// Version number for optimistic concurrency control.
        /// </summary>
        public long Version { get; private set; }

        /// <summary>
        /// UTC timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// UTC timestamp when the entity was last modified.
        /// </summary>
        public DateTime? ModifiedAt { get; private set; }

        /// <summary>
        /// Constructor with identity. Sets CreatedAt and initial Version.
        /// </summary>
        /// <param name="id">The entity ID</param>
        protected EntityBase(TId id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null.");

            Id = id;
            CreatedAt = DateTime.UtcNow;
            Version = 1;
        }

        /// <summary>
        /// Protected parameterless constructor for ORM/ODM support.
        /// </summary>
        protected EntityBase()
        {
            // For ORM tools that require a parameterless constructor
        }

        /// <summary>
        /// Marks entity as modified by updating ModifiedAt and incrementing Version.
        /// Call this method when entity state changes.
        /// </summary>
        public void MarkModified()
        {
            ModifiedAt = DateTime.UtcNow;
            Version++;
        }

        /// <summary>
        /// Validation hook for derived classes to implement entity-specific validation logic.
        /// </summary>
        public virtual void Validate()
        {
            // Override in derived classes for specific validation
        }

        #region Equality and Hashing

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals(obj as EntityBase<TId>);
        }

        public bool Equals(EntityBase<TId> other)
        {
            if (other is null) return false;

            // If both have default IDs, they are not equal (transient entities)
            if (IsTransient() && other.IsTransient())
                return false;

            // Entities are equal if they have the same Id and type
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id == null ? base.GetHashCode() : Id.GetHashCode();
        }

        public static bool operator ==(EntityBase<TId> left, EntityBase<TId> right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }

        public static bool operator !=(EntityBase<TId> left, EntityBase<TId> right) => !(left == right);

        #endregion

        /// <summary>
        /// Determines if entity is transient (Id is default value).
        /// </summary>
        /// <returns>True if entity is transient, else false.</returns>
        public bool IsTransient()
        {
            return EqualityComparer<TId>.Default.Equals(Id, default);
        }

        /// <summary>
        /// Returns a string representation of the entity.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
    }
}
