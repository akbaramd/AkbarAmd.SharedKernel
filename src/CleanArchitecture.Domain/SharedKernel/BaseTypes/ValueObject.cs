/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Base Types
 * ValueObject base class implementing equality, comparison, cloning, validation, and event notification.
 * Year: 2025
 */

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using CleanArchitecture.Domain.SharedKernel.Rules;
using CleanArchitecture.Domain.SharedKernel.Exceptions;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ValueObject : IEquatable<ValueObject>, IComparable<ValueObject>, ICloneable, IFormattable
    {
        [JsonIgnore]
        private IReadOnlyList<object> _equalityComponents;

        [JsonIgnore]
        private int? _hashCode;

        private readonly object _cacheLock = new();

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected ValueObject()
        {
            Validate(); // Validate on construction ensures always valid
        }

        protected abstract IEnumerable<object> GetEqualityComponents();

        private IReadOnlyList<object> EqualityComponents
        {
            get
            {
                if (_equalityComponents == null)
                {
                    lock (_cacheLock)
                    {
                        if (_equalityComponents == null)
                        {
                            _equalityComponents = GetEqualityComponents().ToList().AsReadOnly();
                        }
                    }
                }
                return _equalityComponents;
            }
        }

        #region Equality and Hashing

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            if (obj.GetType() != GetType() && !IsAssignableFromProxy(obj.GetType()))
                return false;

            if (obj is not ValueObject other) return false;

            return EqualityComponents.SequenceEqual(other.EqualityComponents);
        }

        public virtual bool Equals(ValueObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other.GetType() != GetType() && !IsAssignableFromProxy(other.GetType()))
                return false;

            return EqualityComponents.SequenceEqual(other.EqualityComponents);
        }

        public override int GetHashCode()
        {
            if (_hashCode.HasValue) return _hashCode.Value;

            lock (_cacheLock)
            {
                if (_hashCode.HasValue) return _hashCode.Value;

                unchecked
                {
                    int hash = 17;
                    foreach (var component in EqualityComponents)
                    {
                        hash = hash * 31 + (component?.GetHashCode() ?? 0);
                    }
                    _hashCode = hash;
                }
                return _hashCode.Value;
            }
        }

        private bool IsAssignableFromProxy(Type otherType)
        {
            var thisType = GetType();
            return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
        }

        #endregion

        #region Comparison

        public int CompareTo(ValueObject other)
        {
            if (other == null) return 1;

            if (GetType() != other.GetType() && !IsAssignableFromProxy(other.GetType()))
                throw new ArgumentException($"Cannot compare ValueObjects of different types: {GetType()} and {other.GetType()}");

            using var thisEnum = EqualityComponents.GetEnumerator();
            using var otherEnum = other.EqualityComponents.GetEnumerator();

            while (thisEnum.MoveNext() && otherEnum.MoveNext())
            {
                var thisComp = thisEnum.Current;
                var otherComp = otherEnum.Current;

                if (thisComp == null && otherComp == null) continue;
                if (thisComp == null) return -1;
                if (otherComp == null) return 1;

                if (thisComp is IComparable thisComparable && otherComp is IComparable otherComparable)
                {
                    int cmp = thisComparable.CompareTo(otherComparable);
                    if (cmp != 0) return cmp;
                }
                else if (!Equals(thisComp, otherComp))
                {
                    throw new InvalidOperationException($"Cannot compare non-IComparable components: {thisComp} and {otherComp}");
                }
            }

            if (thisEnum.MoveNext()) return 1;
            if (otherEnum.MoveNext()) return -1;
            return 0;
        }

        #endregion

        #region Operators

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);

        public static bool operator <(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null)) return right is not null;
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null)) return true;
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null)) return false;
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(ValueObject? left, ValueObject? right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.CompareTo(right) >= 0;
        }

        #endregion

        #region Cloning

        public virtual object Clone() => MemberwiseClone();

        public virtual T Clone<T>() where T : ValueObject => (T)Clone();

        protected virtual T DeepClone<T>() where T : ValueObject
        {
            var clone = (T)Activator.CreateInstance(GetType(), nonPublic: true)
                        ?? throw new InvalidOperationException($"Unable to create clone of type {GetType()}");

            var fields = GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            foreach (var field in fields)
            {
                var value = field.GetValue(this);

                if (value is ICloneable cloneable)
                {
                    field.SetValue(clone, cloneable.Clone());
                }
                else
                {
                    field.SetValue(clone, value);
                }
            }

            return clone;
        }

        #endregion

        #region Validation

        /// <summary>
        /// Hook to validate ValueObject on construction.
        /// Override to implement custom validation logic.
        /// </summary>
        public virtual void Validate()
        {
            // Default empty implementation, override in derived classes
        }

        /// <summary>
        /// Helper for null check.
        /// </summary>
        protected static void ValidateNotNull(object value, string parameterName, [CallerMemberName] string caller = null)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName, $"Parameter '{parameterName}' cannot be null. Called from {caller}.");
        }

        /// <summary>
        /// Helper for empty string check.
        /// </summary>
        protected static void ValidateNotEmpty(string value, string parameterName, [CallerMemberName] string caller = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Parameter '{parameterName}' cannot be null or whitespace. Called from {caller}.", parameterName);
        }

        /// <summary>
        /// Checks a business rule and throws if broken.
        /// </summary>
        protected static void CheckRule(IBusinessRule rule, [CallerMemberName] string caller = null)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            if (!rule.IsSatisfied())
                throw new BusinessRuleValidationException(rule);
        }

        #endregion

        #region Utilities

        public override string ToString()
        {
            var components = EqualityComponents.Select(c => c?.ToString() ?? "null");
            return $"{GetType().Name}[{string.Join(", ", components)}]";
        }

        public virtual string ToString(string format, IFormatProvider formatProvider) => ToString();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => ToString();

        protected static bool EqualsList<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            return left.SequenceEqual(right);
        }

        public static IDictionary<TKey, TValue> ToDictionary<TValue, TKey>(IEnumerable<TValue> values, Func<TValue, TKey> keySelector) where TValue : ValueObject
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            return values.ToDictionary(keySelector);
        }

        /// <summary>
        /// Raises ValueChanged event for observers.
        /// Should be called by derived classes when internal state changes.
        /// </summary>
        protected void OnValueChanged(object oldValue, object newValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue, newValue));

            _equalityComponents = null;
            _hashCode = null;
        }

        private int CalculateHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var component in EqualityComponents)
                {
                    hash = hash * 31 + (component?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }

        #endregion

        #region Implicit conversions

        public static implicit operator string(ValueObject valueObject) => valueObject?.ToString();

        #endregion
    }

    public sealed class ValueChangedEventArgs : EventArgs
    {
        public object OldValue { get; }
        public object NewValue { get; }

        public ValueChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class ValidatedNotNullAttribute : Attribute { }
}
