/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Base Types
 * Enumeration base class for rich enums with enterprise features.
 * Year: 2025
 */

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes
{
    /// <summary>
    /// Base class for rich enumerations (a.k.a smart enums) with full enterprise features.
    /// Supports equality, comparison, parsing, serialization, metadata, and ORM/ODM compatibility.
    /// Designed to be thread-safe and highly performant.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class Enumeration : IComparable, IEquatable<Enumeration>
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<Enumeration>> Cache = new();

        public string Name { get; }

        public int Id { get; }

        /// <summary>
        /// Optional description to explain the enumeration value.
        /// Can be null or empty if no description is provided.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Creates a new enumeration instance.
        /// Use protected constructor to prevent external instantiation.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        protected Enumeration(int id, string name, string? description = null)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
        }

        /// <summary>
        /// Returns all defined instances of a given Enumeration type.
        /// Uses caching for performance.
        /// </summary>
        public static IReadOnlyList<T> GetAll<T>() where T : Enumeration
        {
            var type = typeof(T);

            if (Cache.TryGetValue(type, out var cached))
                return (IReadOnlyList<T>)cached;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var instances = fields
                .Where(f => f.FieldType == type)
                .Select(f => (T)f.GetValue(null))
                .OrderBy(x => x.Id)
                .ToList()
                .AsReadOnly();

            Cache[type] = instances;

            return instances;
        }

        /// <summary>
        /// Try to parse Enumeration by Id value.
        /// </summary>
        public static bool TryFromValue<T>(int value, out T result) where T : Enumeration
        {
            result = GetAll<T>().FirstOrDefault(item => item.Id == value);
            return result != null;
        }

        /// <summary>
        /// Parse Enumeration by Id value or throw.
        /// </summary>
        public static T FromValue<T>(int value) where T : Enumeration
        {
            if (TryFromValue<T>(value, out var result))
                return result;

            throw new InvalidOperationException($"'{value}' is not a valid value in {typeof(T)}");
        }

        /// <summary>
        /// Try to parse Enumeration by Name (case insensitive).
        /// </summary>
        public static bool TryFromName<T>(string name, out T result) where T : Enumeration
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                result = null;
                return false;
            }

            result = GetAll<T>().FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
            return result != null;
        }

        /// <summary>
        /// Parse Enumeration by Name or throw.
        /// </summary>
        public static T FromName<T>(string name) where T : Enumeration
        {
            if (TryFromName<T>(name, out var result))
                return result;

            throw new InvalidOperationException($"'{name}' is not a valid name in {typeof(T)}");
        }

        /// <summary>
        /// Checks if the specified Id exists in the Enumeration.
        /// </summary>
        public static bool IsValidValue<T>(int value) where T : Enumeration => GetAll<T>().Any(e => e.Id == value);

        /// <summary>
        /// Checks if the specified Name exists in the Enumeration.
        /// </summary>
        public static bool IsValidName<T>(string name) where T : Enumeration => GetAll<T>().Any(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns dictionary of Id to Name for the Enumeration type.
        /// </summary>
        public static IReadOnlyDictionary<int, string> ToDictionary<T>() where T : Enumeration
        {
            return GetAll<T>().ToDictionary(item => item.Id, item => item.Name);
        }

        /// <summary>
        /// Returns the string representation (Name).
        /// </summary>
        public override string ToString() => Name;

        /// <summary>
        /// Returns a display string combining Name, Id and optionally Description.
        /// </summary>
        public virtual string ToDisplayString()
        {
            return string.IsNullOrWhiteSpace(Description)
                ? $"{Name} ({Id})"
                : $"{Name} ({Id}) - {Description}";
        }

        /// <summary>
        /// Equality comparison by Type and Id.
        /// </summary>
        public override bool Equals(object? obj) => Equals(obj as Enumeration);

        /// <summary>
        /// Equality comparison by Type and Id.
        /// </summary>
        public bool Equals(Enumeration? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return GetType() == other.GetType() && Id == other.Id;
        }

        /// <summary>
        /// Hash code combines Type and Id for uniqueness.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(GetType(), Id);

        /// <summary>
        /// Compares two Enumeration instances by Id.
        /// Requires same type.
        /// </summary>
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj is Enumeration other)
            {
                if (GetType() != other.GetType())
                    throw new ArgumentException($"Cannot compare different Enumeration types: {GetType()} and {other.GetType()}");

                return Id.CompareTo(other.Id);
            }

            throw new ArgumentException("Object is not an Enumeration");
        }

        #region Operators

        public static bool operator ==(Enumeration? left, Enumeration? right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator !=(Enumeration? left, Enumeration? right) => !(left == right);

        public static bool operator <(Enumeration? left, Enumeration? right)
        {
            if (ReferenceEquals(left, null))
                return right is not null;

            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Enumeration? left, Enumeration? right)
        {
            if (ReferenceEquals(left, null))
                return true;

            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Enumeration? left, Enumeration? right)
        {
            if (ReferenceEquals(left, null))
                return false;

            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Enumeration? left, Enumeration? right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.CompareTo(right) >= 0;
        }

        #endregion

        #region Implicit conversions

        /// <summary>
        /// Implicit conversion to int (Id).
        /// </summary>
        public static implicit operator int(Enumeration enumeration) => enumeration.Id;

        /// <summary>
        /// Implicit conversion to string (Name).
        /// </summary>
        public static implicit operator string(Enumeration enumeration) => enumeration.Name;

        #endregion

        #region Additional helpers

        /// <summary>
        /// Returns all Ids in the Enumeration type.
        /// </summary>
        public static IEnumerable<int> GetValues<T>() where T : Enumeration
        {
            return GetAll<T>().Select(e => e.Id);
        }

        /// <summary>
        /// Returns all Names in the Enumeration type.
        /// </summary>
        public static IEnumerable<string> GetNames<T>() where T : Enumeration
        {
            return GetAll<T>().Select(e => e.Name);
        }

        /// <summary>
        /// Tries to parse string to Enumeration by Name or Id (int).
        /// </summary>
        public static bool TryParse<T>(string input, out T result) where T : Enumeration
        {
            result = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Try parse as int Id first
            if (int.TryParse(input, out int intValue))
            {
                return TryFromValue<T>(intValue, out result);
            }

            // Try parse as name (case insensitive)
            return TryFromName<T>(input, out result);
        }

        /// <summary>
        /// Parses string to Enumeration by Name or Id or throws.
        /// </summary>
        public static T Parse<T>(string input) where T : Enumeration
        {
            if (TryParse<T>(input, out var result))
                return result;

            throw new InvalidOperationException($"'{input}' is not a valid Name or Id in {typeof(T)}");
        }

        #endregion

        #region JSON Serialization support

        /// <summary>
        /// Supports deserialization with System.Text.Json or Newtonsoft.Json.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        protected Enumeration(int id, string name, bool _dummy = false)
            : this(id, name, null)
        {
        }

        #endregion

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => ToDisplayString();
    }
}
