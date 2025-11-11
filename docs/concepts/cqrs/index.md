# CQRS (Command Query Responsibility Segregation) Concepts

CQRS is a pattern that separates read and write operations. Commands change state, while queries return data without side effects.

## Overview

This section covers CQRS concepts and patterns:

- **[CQRS](cqrs.md)** - Command Query Responsibility Segregation pattern

## Required Modules

To use CQRS concepts, you need to install:

- **AkbarAmd.SharedKernel.Application** - CQRS contracts and handler base classes
- **AkbarAmd.SharedKernel.Domain** - Domain models (for commands/queries that work with domain entities)
- **AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore** - Repository implementations (optional, if using repositories in handlers)

## Installation

```bash
# Application module (required for CQRS)
dotnet add package AkbarAmd.SharedKernel.Application

# Domain module (required if working with domain entities)
dotnet add package AkbarAmd.SharedKernel.Domain

# EF Core infrastructure (optional, for repository implementations)
dotnet add package AkbarAmd.SharedKernel.Infrastructure.EntityFrameworkCore
```

## Getting Started

1. Read the [CQRS](cqrs.md) guide to understand the pattern
2. Create commands for write operations
3. Create queries for read operations
4. Implement command and query handlers
5. Use with [Repository](../ddd/repository.md) pattern for data access

## Related Topics

- [DDD Concepts](../ddd/index.md) - Learn about Domain-Driven Design patterns
- [Application Module](../../modules/application.md) - See CQRS contracts and handlers
- [Domain Events](../ddd/domain-events.md) - Raise domain events from commands

