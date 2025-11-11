# Value Objects

Value Objects are immutable objects that are defined by their attributes rather than their identity. They represent descriptive aspects of the domain with no conceptual identity.

## Installation

To use Value Objects, install the following module:

```bash
# Core domain module (required for ValueObject base class)
dotnet add package AkbarAmd.SharedKernel.Domain
```

## Overview

Value Objects:
- Have no identity (equality is based on values)
- Are immutable (cannot be changed after creation)
- Represent domain concepts
- Can be shared across entities
- Are validated at construction

## ValueObject Base Class

```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public static bool operator ==(ValueObject? left, ValueObject? right);
    public static bool operator !=(ValueObject? left, ValueObject? right);
}
```

## Creating Value Objects

### Simple Value Object

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
            
        if (!value.Contains("@"))
            throw new ArgumentException("Invalid email format", nameof(value));
            
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
```

### Complex Value Object

```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode cannot be empty", nameof(zipCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}
```

### Money Value Object

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");
            
        return new Money(Amount - other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

## Using Value Objects in Entities

```csharp
public class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; }
    public Address? Address { get; private set; }

    private User() { }

    public User(Email email)
    {
        Id = Guid.NewGuid();
        Email = email;
    }

    public void UpdateAddress(Address address)
    {
        Address = address; // Value objects are immutable, so we can replace them
    }

    public void ChangeEmail(Email newEmail)
    {
        Email = newEmail;
    }
}
```

## Enumeration Pattern

The domain module also provides an `Enumeration` base class for smart enums:

```csharp
public class OrderStatus : Enumeration
{
    public static OrderStatus Draft = new(1, "Draft");
    public static OrderStatus Confirmed = new(2, "Confirmed");
    public static OrderStatus Shipped = new(3, "Shipped");
    public static OrderStatus Delivered = new(4, "Delivered");
    public static OrderStatus Cancelled = new(5, "Cancelled");

    public OrderStatus(int id, string name) : base(id, name)
    {
    }

    public static OrderStatus FromId(int id)
    {
        return GetAll<OrderStatus>().FirstOrDefault(s => s.Id == id)
            ?? throw new ArgumentException($"Invalid OrderStatus id: {id}");
    }
}

// Usage
public class Order : AggregateRoot<Guid>
{
    public OrderStatus Status { get; private set; }

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
    }
}
```

## Benefits

- **Immutability**: Prevents accidental modifications
- **Validation**: Ensures valid values at construction
- **Encapsulation**: Encapsulates related data and behavior
- **Reusability**: Can be shared across multiple entities
- **Type Safety**: Strong typing prevents primitive obsession
- **Equality**: Value-based equality simplifies comparisons

## Best Practices

1. **Make them immutable**: Value objects should never change after creation
2. **Validate at construction**: Ensure all validation happens in the constructor
3. **Override equality**: Use `GetEqualityComponents()` to define equality
4. **Override ToString()**: Provide meaningful string representation
5. **Keep them small**: Value objects should represent a single concept
6. **No identity**: Value objects have no identity, only values matter

## Related Topics

- [Aggregate Root](aggregate-root.md) - Use value objects in aggregate roots
- [DDD Overview](index.md) - Learn about DDD concepts
- [Domain Module](../../modules/domain.md) - See the ValueObject base class

