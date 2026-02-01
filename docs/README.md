# Shared Kernels Documentation

Welcome to the Shared Kernels documentation. This documentation provides comprehensive guides for using the Shared Kernel modules in your .NET applications.

## Getting Started

Start by exploring the [Modules](modules/) section to understand what each module provides, then dive into the [Concepts](concepts/) section to learn about key patterns and practices.

## Modules

- **[AkbarAmd.SharedKernel.Application](modules/application.md)** - CQRS, MediatR, and application services
- **[AkbarAmd.SharedKernel.Domain](modules/domain.md)** - Domain models, entities, and business logic
- **[AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore](modules/infrastructure.md)** - EF Core repository implementations

## Concepts

### Application Patterns

- **[Application Patterns Overview](concepts/application/index.md)** - Application layer patterns overview
- **[ServiceResult](concepts/application/service-result.md)** - Service result pattern for operation outcomes
- **[ServiceBase](concepts/application/service-base.md)** - Base class for service implementations

### DDD Concepts

- **[DDD Overview](concepts/ddd/index.md)** - Domain-Driven Design concepts overview
- **[Repository](concepts/ddd/repository.md)** - Repository pattern for data access
- **[Specifications](concepts/ddd/specifications.md)** - Specification pattern for complex queries
- **[Aggregate Root](concepts/ddd/aggregate-root.md)** - Aggregate roots and domain entities
- **[Domain Events](concepts/ddd/domain-events.md)** - Domain events for cross-aggregate communication
- **[Value Objects](concepts/ddd/value-objects.md)** - Immutable value objects

### CQRS Concepts

- **[CQRS Overview](concepts/cqrs/index.md)** - CQRS concepts overview
- **[CQRS](concepts/cqrs/cqrs.md)** - Command Query Responsibility Segregation

## Quick Links

- [GitHub Repository](https://github.com/akbaramd/AkbarAmd.SharedKernel)
- [Installation Guide](modules/application.md#installation)
