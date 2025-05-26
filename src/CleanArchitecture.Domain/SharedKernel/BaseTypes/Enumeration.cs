using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes;

public abstract class Enumeration : IComparable
{
    public string Name { get; }
    public int Id { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Enumeration otherValue)
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static T FromValue<T>(int value) where T : Enumeration
    {
        var matchingItem = Parse<T, int>(value, "value", item => item.Id == value);
        return matchingItem;
    }

    public static T FromName<T>(string name) where T : Enumeration
    {
        var matchingItem = Parse<T, string>(name, "name", item => item.Name == name);
        return matchingItem;
    }

    private static T Parse<T, K>(K value, string description, Func<T, bool> predicate) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(predicate);

        if (matchingItem == null)
            throw new InvalidOperationException($"'{value}' is not a valid {description} in {typeof(T)}");

        return matchingItem;
    }

    public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

    public static bool operator ==(Enumeration left, Enumeration right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    public static bool operator !=(Enumeration left, Enumeration right)
    {
        return !(left == right);
    }

    public static bool operator <(Enumeration left, Enumeration right)
    {
        return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
    }

    public static bool operator <=(Enumeration left, Enumeration right)
    {
        return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    }

    public static bool operator >(Enumeration left, Enumeration right)
    {
        return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
    }

    public static bool operator >=(Enumeration left, Enumeration right)
    {
        return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    }
} 