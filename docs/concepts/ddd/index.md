# DDD (Domain-Driven Design) Concepts

Domain-Driven Design (DDD) is an approach to software development that centers the development on programming a domain model that has a rich understanding of the processes and rules of a domain.

## Overview

This section covers the core DDD concepts and patterns implemented in the Shared Kernel modules:

- **[Repository](repository.md)** - Abstraction layer for data access
- **[Specifications](specifications.md)** - Encapsulate business rules and query logic
- **[Aggregate Root](aggregate-root.md)** - Consistency boundaries and domain entities
- **[Domain Events](domain-events.md)** - Communicate changes between aggregates
- **[Value Objects](value-objects.md)** - Immutable domain concepts

## Required Modules

To use DDD concepts, you need to install:

- **AkbarAmd.SharedKernel.Domain** - Core domain models and interfaces
- **AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore** - Repository implementations (optional, for EF Core)

## Installation

```bash
# Core domain module (required)
dotnet add package AkbarAmd.SharedKernel.Domain

# EF Core infrastructure (optional, for repository implementations)
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Getting Started

1. Start with [Aggregate Root](aggregate-root.md) to understand domain entities
2. Learn about [Value Objects](value-objects.md) for domain concepts
3. Use [Repository](repository.md) pattern for data access
4. Build complex queries with [Specifications](specifications.md)
5. Communicate changes with [Domain Events](domain-events.md)

## Related Topics

- [CQRS Concepts](../cqrs/index.md) - Learn about CQRS patterns
- [Domain Module](../../modules/domain.md) - See the domain module details
- [Infrastructure Module](../../modules/infrastructure.md) - See repository implementations

