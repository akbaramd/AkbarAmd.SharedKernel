# Specification Pattern Documentation

> Comprehensive guide for using the Specification Pattern in Clean Architecture

## Overview

The Specification Pattern provides a powerful way to encapsulate business rules and query logic in a reusable, testable, and maintainable manner. This implementation supports:

- ✅ **Complex Criteria Building** - AND, OR, NOT operations with grouping
- ✅ **Fluent API** - Clean, chainable syntax for building specifications
- ✅ **Multi-level Sorting** - OrderBy, ThenBy with null handling
- ✅ **Pagination** - Built-in support for paging queries
- ✅ **Eager Loading** - Include related entities efficiently
- ✅ **EF Core Integration** - Optimized for Entity Framework Core

## Quick Start

```csharp
// Simple specification
var spec = new FluentSpecificationBuilder<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Page(1, 10)
    .Build();

var products = await repository.FindAsync(spec);
```

## Table of Contents

- [Getting Started](getting-started.md)
- [Basic Usage](basic-usage.md)
- [Fluent API](fluent-api.md)
- [Database Patterns](database-patterns.md)
- [Enterprise Patterns](enterprise-patterns.md)
- [Advanced Examples](advanced-examples.md)
- [API Reference](api-reference.md)

