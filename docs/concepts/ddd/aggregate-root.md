# Aggregate Root

An Aggregate Root is a cluster of domain objects that are treated as a single unit for data changes. It serves as the entry point for accessing the aggregate and enforces business invariants.

## Installation

To use Aggregate Roots, install the following module:

```bash
# Core domain module (required for aggregate root base classes)
dotnet add package AkbarAmd.SharedKernel.Domain
```

## Overview

Aggregate Roots:
- Act as the single entry point to an aggregate
- Enforce business invariants and consistency rules using business rules
- Business rule validation via inherited `CheckRule(IBusinessRule rule)` method from Entity base class
- Manage domain events
- Control access to entities within the aggregate
- Handle optimistic concurrency control

## Base Classes

The domain module provides several aggregate root base classes:

### AggregateRoot<TKey>

Base aggregate root with domain events and concurrency control:

```csharp
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    public bool HasPendingEvents { get; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    public long Version { get; }
    public byte[] RowVersion { get; set; }
    
    protected void RaiseDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();
    public void IncrementVersion();
}
```

### CreatableAggregateRoot<TKey>

Adds creation tracking:

```csharp
public abstract class CreatableAggregateRoot<TKey> : AggregateRoot<TKey>
{
    public DateTime CreatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
}
```

### ModifiableAggregateRoot<TKey>

Adds modification tracking:

```csharp
public abstract class ModifiableAggregateRoot<TKey> : CreatableAggregateRoot<TKey>
{
    public DateTime? ModifiedAt { get; protected set; }
    public string? ModifiedBy { get; protected set; }
}
```

### SoftDeletableAggregateRoot<TKey>

Adds soft delete support:

```csharp
public abstract class SoftDeletableAggregateRoot<TKey> : ModifiableAggregateRoot<TKey>
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }
    
    public void SoftDelete(string deletedBy);
    public void Restore();
}
```

### FullAggregateRoot<TKey>

Complete aggregate root with all features:

```csharp
public abstract class FullAggregateRoot<TKey> : SoftDeletableAggregateRoot<TKey>
{
    // Includes: creation, modification, soft delete, concurrency control, domain events
}
```

## Usage Examples

### Basic Aggregate Root with Business Rules

```csharp
using AkbarAmd.SharedKernel.Domain.AggregateRoots;
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    private User() { } // For EF Core

    public User(string email, string name)
    {
        Id = Guid.NewGuid();
        
        // Business rule validation - inherited from Entity base class
        CheckRule(new EmailCannotBeEmptyRule(email));
        CheckRule(new NameCannotBeEmptyRule(name));
        
        Email = email;
        Name = name;
        IsActive = true;
        
        AddDomainEvent(new UserCreatedEvent(Id, Email));
    }

    public void UpdateName(string newName)
    {
        CheckRule(new NameCannotBeEmptyRule(newName));
        Name = newName;
        RaiseDomainEvent(new UserNameChangedEvent(Id, newName));
    }
}

// Business Rule Implementation
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class NameCannotBeEmptyRule : BaseBusinessRule
{
    private readonly string _name;
    
    public NameCannotBeEmptyRule(string name)
    {
        _name = name;
    }
    
    public override bool IsSatisfied() => !string.IsNullOrWhiteSpace(_name);
    public override string Message => "Name cannot be empty";

    public void Deactivate()
    {
        if (!IsActive)
            return;
            
        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id));
    }
}
```

### Aggregate Root with Full Tracking

```csharp
public class Order : FullAggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = new();
    
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order() { }

    public Order(Guid id, string createdBy) : base(id, createdBy)
    {
        Status = OrderStatus.Draft;
        RaiseDomainEvent(new OrderCreatedEvent(Id));
    }

    public void AddItem(Product product, int quantity)
    {
        CheckRule(new OrderCanBeModifiedRule(Status));
        var item = new OrderItem(product.Id, product.Price, quantity);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.Subtotal);
        
        RaiseDomainEvent(new OrderItemAddedEvent(Id, item.ProductId, quantity));
    }

    public void Confirm()
    {
        CheckRule(new OrderCannotBeEmptyRule(_items.Count));
        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(Id, TotalAmount));
    }
}

// Business Rule Implementations
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class OrderCanBeModifiedRule : BaseBusinessRule
{
    private readonly OrderStatus _status;
    
    public OrderCanBeModifiedRule(OrderStatus status)
    {
        _status = status;
    }
    
    public override bool IsSatisfied() => _status == OrderStatus.Draft;
    public override string Message => "Cannot modify confirmed order";
}

public class OrderCannotBeEmptyRule : BaseBusinessRule
{
    private readonly int _itemCount;
    
    public OrderCannotBeEmptyRule(int itemCount)
    {
        _itemCount = itemCount;
    }
    
    public override bool IsSatisfied() => _itemCount > 0;
    public override string Message => "Cannot confirm empty order";
}

public class EmailCannotBeEmptyRule : BaseBusinessRule
{
    private readonly string _email;
    
    public EmailCannotBeEmptyRule(string email)
    {
        _email = email;
    }
    
    public override bool IsSatisfied() => !string.IsNullOrWhiteSpace(_email);
    public override string Message => "Email cannot be empty";
}
```

### Working with Domain Events

```csharp
public class User : AggregateRoot<Guid>
{
    public void ChangeEmail(string newEmail)
    {
        CheckRule(new EmailCannotBeEmptyRule(newEmail));
        var oldEmail = Email;
        Email = newEmail;
        
        // Raise domain event
        RaiseDomainEvent(new UserEmailChangedEvent(Id, oldEmail, newEmail));
    }
}

// In your application service or handler
public class ChangeEmailCommandHandler : CommandHandler<ChangeEmailCommand>
{
    protected override async Task ProcessAsync(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        user.ChangeEmail(request.NewEmail);
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
        await _userRepository.SaveAsync(cancellationToken);
        
        // Publish domain events
        await PublishDomainEventsAsync(user, cancellationToken);
    }
}
```

## Key Principles

1. **Single Entry Point**: Access entities within the aggregate only through the aggregate root
2. **Invariant Enforcement**: Aggregate root ensures business rules are always satisfied
3. **Transaction Boundary**: The aggregate defines the boundary for transactions
4. **Domain Events**: Use domain events to communicate changes to other aggregates
5. **Optimistic Concurrency**: Use version/RowVersion for concurrency control

## Benefits

- **Consistency**: Ensures business invariants are maintained
- **Encapsulation**: Hides internal structure of the aggregate
- **Event Sourcing Ready**: Built-in domain event management
- **Audit Trail**: Comprehensive tracking of changes
- **Concurrency Control**: Built-in optimistic concurrency support

## Related Topics

- [Domain Events](domain-events.md) - Learn about domain events
- [Repository](repository.md) - Work with aggregate roots in repositories
- [Value Objects](value-objects.md) - Use value objects in aggregate roots
- [DDD Overview](index.md) - Learn about DDD concepts
- [Domain Module](../../modules/domain.md) - See all aggregate root base classes
