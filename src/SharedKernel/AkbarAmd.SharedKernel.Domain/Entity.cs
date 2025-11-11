using System.Runtime.CompilerServices;
using AkbarAmd.SharedKernel.Domain.Contracts;
using AkbarAmd.SharedKernel.Domain.Exceptions;

namespace AkbarAmd.SharedKernel.Domain
{
    /// <summary>
    /// Base class for all entities without generic constraints.
    /// Provides core entity functionality including business rule validation
    /// and key management through virtual methods.
    /// </summary>
    public abstract class Entity : IEntity
    {
        #region Constructors

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// Should only be used by infrastructure components.
        /// </summary>
        protected Entity()
        {
        }

        #endregion

        #region Key Management

        /// <summary>
        /// Gets the primary keys that identify this entity.
        /// Override in derived classes to provide specific key values.
        /// </summary>
        /// <returns>Array of objects representing the entity's primary keys.</returns>
        public virtual object[] GetKeys()
        {
            return Array.Empty<object>();
        }

        #endregion

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
            return obj is Entity other && GetKeys().SequenceEqual(other.GetKeys());
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current entity.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(GetType(), GetKeys());
        }

        #endregion

        #region Operators

        /// <summary>
        /// Equality operator for entities.
        /// </summary>
        /// <param name="left">Left entity.</param>
        /// <param name="right">Right entity.</param>
        /// <returns>True if entities are equal; otherwise, false.</returns>
        public static bool operator ==(Entity? left, Entity? right)
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
        public static bool operator !=(Entity? left, Entity? right) => !(left == right);

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
        public override string ToString() => $"{GetType().Name} [Keys={string.Join(",", GetKeys())}]";

        #endregion
    }

    /// <summary>
    /// Base class for entities with a generic identity type.
    /// Provides core entity functionality including identity management,
    /// equality comparison, and business rule validation.
    /// Designed to work seamlessly with Entity Framework Core.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identity.</typeparam>
    public abstract class Entity<TId> : Entity, IEntity<TId> 
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// The entity's identifier. 
        /// Setter is protected to allow derived classes to modify during construction,
        /// but EF Core can still access it for mapping and persistence.
        /// </summary>
        public TId Id { get; protected set; }

        /// <summary>
        /// Constructor for setting up entity with ID.
        /// Validates that ID is not null or default value.
        /// This constructor is used when creating new entities with a known ID.
        /// </summary>
        /// <param name="id">The entity's unique identifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
        /// <exception cref="ArgumentException">Thrown when id is the default value.</exception>
        protected Entity(TId id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id), "Entity ID cannot be null.");
            
            if (EqualityComparer<TId>.Default.Equals(id, default(TId)))
                throw new ArgumentException("Entity ID cannot be the default value.", nameof(id));
            
            Id = id;
        }

        /// <summary>
        /// Default constructor for ORM/ODM support.
        /// This constructor is required by Entity Framework Core for materialization.
        /// The ID will be set by EF Core during materialization or by derived classes.
        /// </summary>
        protected Entity()
        {
            // EF Core will set the ID during materialization
            // Derived classes should validate the ID in their business logic if needed
        }

        #region Key Management

        /// <summary>
        /// Gets the primary keys that identify this entity.
        /// Override to provide the ID as the primary key.
        /// </summary>
        /// <returns>Array containing the entity's ID.</returns>
        public override object[] GetKeys()
        {
            return new object[] { Id };
        }

        #endregion

        #region Equality and Comparison

        /// <summary>
        /// Determines whether the specified entity is equal to the current entity.
        /// </summary>
        /// <param name="other">The entity to compare with the current entity.</param>
        /// <returns>True if the specified entity is equal to the current entity; otherwise, false.</returns>
        public bool Equals(IEntity<TId>? other) =>
            other is not null && Id.Equals(other.Id);

        #endregion

        #region String Representation

        /// <summary>
        /// Returns a string representation of the entity.
        /// </summary>
        /// <returns>A string that represents the current entity.</returns>
        public override string ToString() => $"{GetType().Name} [Id={Id}]";

        #endregion
    }


  
}
