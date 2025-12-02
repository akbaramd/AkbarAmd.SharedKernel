# AkbarAmd.SharedKernel.Domain

The Domain layer is the core of the Shared Kernel, containing business logic, domain models, and domain rules. This module provides foundational building blocks for implementing Domain-Driven Design (DDD) patterns in .NET applications.

## Overview

This module defines:
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

## Usage Example

### Creating an Aggregate Root

```csharp
public class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string Name { get; private set; }

    private User() { } // For EF Core

    public User(string email, string name)
    {
        Id = Guid.NewGuid();
        Email = email;
        Name = name;
        
        // Raise domain event
        RaiseDomainEvent(new UserCreatedEvent(Id, Email));
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
}
```

## Related Concepts

- [Aggregate Root](concepts/ddd/aggregate-root.md) - Learn about aggregate roots
- [Domain Events](concepts/ddd/domain-events.md) - Understand domain events
- [Value Objects](concepts/ddd/value-objects.md) - See how value objects work
- [Business Rules vs Specifications](concepts/ddd/business-rules-vs-specifications.md) - Understand when to use business rules vs specifications
- [Specifications](concepts/ddd/specifications.md) - Build complex queries
- [Repository](concepts/ddd/repository.md) - Use repositories with domain entities

## Dependencies

- .NET 8.0 or later
- No external dependencies (pure domain layer)

