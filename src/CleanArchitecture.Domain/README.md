# Enterprise Domain Layer Structure

This document outlines the comprehensive structure of the Enterprise Domain Layer, designed following Domain-Driven Design (DDD) principles and clean architecture patterns.

## Directory Structure

```
Enterprise.Domain/
│
├─ SharedKernel/                       ← 100% Shared Types and Tools
│   ├─ Abstractions/                   IEntity, IAggregateRoot, IValueObject
│   ├─ BaseTypes/                      EntityBase, ValueObjectBase, Enumeration
│   ├─ Guards/                         Guard, Ensure
│   ├─ ResultTypes/                    Result, Maybe
│   ├─ Events/                         IDomainEvent, DomainEventDispatcher
│   ├─ Rules/                          IBusinessRule, BusinessRuleViolation
│   ├─ Specifications/                 ISpecification, AndSpec, OrSpec
│   ├─ Extensions/                     DateTimeExtensions, EnumExtensions
│   ├─ Constants/                      RegexPatterns, GlobalConsts
│   ├─ Exceptions/                     DomainException, ValidationException
│   └─ README.md
│
├─ SeedWork/                           ← Non-essential and Low-change Helper Components
│   ├─ Interfaces/                     IClock, ICorrelationId
│   ├─ Outbox/                         OutboxMessage, IOutboxRepository
│   └─ README.md
│
├─ Contexts/                           ← All Bounded Contexts in One Level
│   │
│   ├─ Catalog/
│   │   ├─ Aggregates/
│   │   │   └─ Product/
│   │   │       ├─ Entities/           Product.cs, Category.cs
│   │   │       ├─ ValueObjects/       Price.cs, Dimension.cs
│   │   │       ├─ Rules/              UniqueSkuRule.cs
│   │   │       ├─ Policies/           PricePolicy.cs, StockPolicy.cs
│   │   │       ├─ Services/           PricingDomainService.cs
│   │   │       ├─ Events/             ProductPriceChanged.cs
│   │   │       └─ Factories/          ProductFactory.cs
│   │   ├─ Interfaces/                 IProductRepository.cs, ICategoryRepository.cs
│   │   ├─ Specifications/             ProductsByStatusSpecification.cs
│   │   └─ README.md
│   │
│   ├─ Orders/
│   │   ├─ Aggregates/
│   │   │   └─ Order/
│   │   │       ├─ Entities/           Order.cs, OrderLine.cs, Address.cs
│   │   │       ├─ ValueObjects/       Money.cs, Discount.cs
│   │   │       ├─ Rules/              OrderCannotBeEmptyRule.cs
│   │   │       ├─ Policies/           PaymentWindowPolicy.cs
│   │   │       ├─ Services/           OrderCalculationDomainService.cs
│   │   │       ├─ Events/             OrderPlaced.cs, OrderCancelled.cs
│   │   │       └─ Factories/          OrderFactory.cs
│   │   ├─ Interfaces/                 IOrderRepository.cs
│   │   ├─ Specifications/             OrdersByCustomerSpecification.cs
│   │   └─ README.md
│   │
│   ├─ Identity/
│   │   ├─ Aggregates/
│   │   │   └─ User/
│   │   │       ├─ Entities/           User.cs, Role.cs, Permission.cs
│   │   │       ├─ ValueObjects/       Email.cs, PasswordHash.cs
│   │   │       ├─ Rules/              UniqueEmailRule.cs
│   │   │       ├─ Policies/           PasswordPolicy.cs
│   │   │       ├─ Services/           RoleAssignmentDomainService.cs
│   │   │       ├─ Events/             UserRegistered.cs
│   │   │       └─ Factories/          UserFactory.cs
│   │   ├─ Interfaces/                 IUserRepository.cs
│   │   ├─ Specifications/             UsersByRoleSpecification.cs
│   │   └─ README.md
│   │
│   └─ Accounting/                     ← Next Context (if needed)
│       └─ …
```

## Layer Description

The Domain layer is the core of the application, implementing the business logic and rules. It follows Domain-Driven Design (DDD) principles and patterns:

### SharedKernel
Contains core abstractions and base types that are shared across all bounded contexts:
- Base types for entities, value objects, and enumerations
- Domain event infrastructure
- Business rule abstractions
- Common specifications
- Shared extensions and constants
- Domain-specific exceptions

### SeedWork
Contains supporting elements that are not core to the domain but provide necessary functionality:
- Infrastructure interfaces
- Outbox pattern implementation for eventual consistency
- Other cross-cutting concerns

### Contexts
Contains all bounded contexts, each representing a distinct business domain:

#### Catalog Context
- Product aggregate with related entities and value objects
- Product-specific business rules and policies
- Catalog-specific domain services and events

#### Orders Context
- Order aggregate with order lines and address value objects
- Order-specific business rules and payment policies
- Order calculation domain services

#### Identity Context
- User aggregate with roles and permissions
- Authentication and authorization rules
- User management domain services

## Domain Modeling Guidelines

1. **Aggregates**
   - Define clear aggregate boundaries
   - Maintain invariants within aggregates
   - Keep aggregates small and focused

2. **Entities**
   - Implement identity equality
   - Encapsulate state changes
   - Validate business rules

3. **Value Objects**
   - Make them immutable
   - Implement value equality
   - Validate at creation

4. **Domain Events**
   - Use for cross-aggregate communication
   - Keep them immutable
   - Include all necessary context

5. **Business Rules**
   - Express explicitly as objects
   - Make them self-contained
   - Use meaningful names

## Best Practices

1. Follow DDD principles strictly
2. Keep bounded contexts isolated
3. Use ubiquitous language within contexts
4. Implement proper validation
5. Use domain events for cross-aggregate communication
6. Keep aggregates consistent
7. Document domain decisions
8. Write unit tests for business rules

## Contributing

1. Understand the domain context before making changes
2. Follow the established patterns
3. Maintain bounded context boundaries
4. Document new domain concepts
5. Add unit tests for business rules
6. Use meaningful names that reflect domain concepts
7. Keep the shared kernel minimal 