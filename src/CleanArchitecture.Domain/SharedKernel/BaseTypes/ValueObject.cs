using System;
using System.Collections.Generic;
using System.Linq;
using CleanArchitecture.Domain.SharedKernel.Rules;
using CleanArchitecture.Domain.SharedKernel.Exceptions;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    public bool Equals(ValueObject other)
    {
        return Equals((object)other);
    }

    public static bool operator ==(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return !(left == right);
    }

    protected static void CheckRule(IBusinessRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        if (!rule.IsSatisfied())
            throw new BusinessRuleValidationException(rule);
    }

    // Helper method for collections
    protected static bool EqualsList<T>(IEnumerable<T> left, IEnumerable<T> right)
    {
        if (left == null || right == null)
            return left == right;

        return left.SequenceEqual(right);
    }
} 