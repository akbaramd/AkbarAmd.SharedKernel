# AkbarAmd.SharedKernel.Domain

Clean Architecture Domain Layer - Core Domain Models, Entities, Value Objects, and Domain Events

## Overview

The Domain layer is the core of Clean Architecture, containing business logic, domain models, and domain rules. This package provides foundational building blocks for implementing Domain-Driven Design (DDD) patterns in .NET applications.

## Purpose

This layer defines:
- **Business entities** and their relationships
- **Business rules** for domain invariants and validation
- **Domain rules** and business invariants
- **Value objects** for domain concepts
- **Domain events** for domain state changes
- **Specifications** for complex business queries
- **Aggregate roots** that enforce consistency boundaries

## Key Components

### Entities & Aggregate Roots
- `Entity<TKey>`: Base entity class with identity management
- `AggregateRoot<TKey>`: Aggregate root with domain events and concurrency control
- `CreatableAggregateRoot<TKey>`: Aggregate root with creation tracking
- `ModifiableAggregateRoot<TKey>`: Aggregate root with modification tracking
- `SoftDeletableAggregateRoot<TKey>`: Aggregate root with soft delete support
- `FullAggregateRoot<TKey>`: Complete aggregate root with all tracking features

### Value Objects
- `ValueObject`: Immutable value object base class
- `Enumeration`: Smart enumeration pattern for domain constants

### Domain Events
- `IDomainEvent`: Interface for domain events
- Built-in domain event management within aggregate roots
- Thread-safe domain event collection

### Business Rules
- `IBusinessRule`: Interface for domain business rules and invariants
- `BaseBusinessRule`: Base class for implementing business rules
- `DomainBusinessRuleValidationException`: Exception thrown when business rules are violated

### Specifications
- `ISpecification<T>`: Specification pattern interface
- `BaseSpecification<T>`: Base class for building specifications
- `FluentSpecificationBuilder<T>`: Fluent API for building query criteria
- `CriteriaChain<T>`: Chain multiple criteria together

### Outbox Pattern
- `OutboxMessage`: Aggregate root for reliable message processing
- Status tracking and retry mechanism
- Domain events for state changes

### Repositories
- `IRepository<T, TKey>`: Generic repository interface
- `IReadOnlyRepository<T, TKey>`: Read-only repository interface

## Features

- **Domain-Driven Design**: Full DDD support with aggregate roots and entities
- **Event Sourcing Ready**: Built-in domain event management
- **Audit Trail**: Comprehensive audit capabilities (creation, modification, deletion)
- **Concurrency Control**: Optimistic concurrency with version tracking
- **Thread Safety**: Thread-safe domain event operations
- **Specification Pattern**: Complex query building with business rules
- **Outbox Pattern**: Reliable message processing for distributed systems

## Installation

```bash
dotnet add package AkbarAmd.SharedKernel.Domain
```

## Usage Examples

### Creating an Aggregate Root with Business Rules

```csharp
using AkbarAmd.SharedKernel.Domain.AggregateRoots;
using AkbarAmd.SharedKernel.Domain.BusinessRules;

public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string Name { get; private set; }

    private User() { } // For EF Core

    public User(string email, string name)
    {
        Id = Guid.NewGuid();
        
        // Business rule validation - CheckRule is inherited from Entity base class
        CheckRule(new EmailCannotBeEmptyRule(email));
        CheckRule(new NameCannotBeEmptyRule(name));
        
        Email = email;
        Name = name;
        
        // Raise domain event
        AddDomainEvent(new UserCreatedEvent(Id, Email));
    }

    public void UpdateName(string newName)
    {
        // Business rule validation
        CheckRule(new NameCannotBeEmptyRule(newName));
        Name = newName;
        AddDomainEvent(new UserNameChangedEvent(Id, newName));
    }
    
    public void Deactivate()
    {
        CheckRule(new UserCanBeDeactivatedRule(IsActive));
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
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
}
```

### Using Value Objects

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new ArgumentException("Invalid email");
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Using Specifications

```csharp
var activeUsersSpec = new CriteriaBuilder<User>()
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .Take(10)
    .Build();

var users = await repository.GetAsync(activeUsersSpec);
```

## Dependencies

- .NET 8.0 or later
- No external dependencies (pure domain layer)

## Architecture Principles

This layer follows Clean Architecture principles:
- **No dependencies** on other layers
- **Pure business logic** without infrastructure concerns
- **Framework-agnostic** domain models
- **Testable** in isolation

## License

MIT License - see LICENSE file for details.

## Author

Akbar Ahmadi Saray - [GitHub](https://github.com/akbarahmadi)
