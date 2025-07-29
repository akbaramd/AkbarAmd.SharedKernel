# Enterprise Application Layer Structure

This document outlines the comprehensive structure of the Enterprise Application Layer, designed following clean architecture principles and advanced enterprise patterns.

## Directory Structure

```
Enterprise.Application/                 ← Advanced Enterprise Application Layer
│
├─ Abstractions/                       ← Dependency Inversion Contracts
│   ├─ Data/
│   │   ├─ Repositories/               IRepository, IReadRepository, IUnitOfWork
│   │   └─ Specifications/             ISpecification, IQuerySpecification
│   ├─ Messaging/
│   │   ├─ Events/                     IEventBus, IIntegrationEventPublisher
│   │   ├─ Commands/                   ICommandBus
│   │   └─ Streams/                    IKafkaProducer, IKafkaConsumer
│   ├─ Jobs/                           IJobScheduler, IBackgroundJob, IJobDispatcher
│   ├─ Caching/                        ICacheProvider, IDistributedLock
│   ├─ Http/                           IRestClient, IGrpcClient, ITypedClient
│   ├─ Security/                       ICurrentUser, IAuthorizationService
│   ├─ Notifications/                  IEmailSender, ISmsSender, IPushSender
│   ├─ Localization/                   IStringLocalizer, ILocalizationService
│   └─ Observability/                  ITracer, IMetrics, ICorrelationContext

├─ CrossCuttings/                      ← Cross-Cutting Concerns
│   ├─ Behaviors/                      Validation, Logging, Transaction, Retry
│   ├─ Filters/                        TenantFilter, SoftDeleteFilter
│   ├─ Policies/                       AuthorizationPolicy, RateLimitPolicy
│   ├─ Pipelines/                      CustomPipelinePieces (e.g., OutboxStep)
│   └─ Interceptors/                   CachingInterceptor, MetricsInterceptor

├─ Common/                             ← Common Helper Code
│   ├─ Exceptions/                     AppException, ValidationException
│   ├─ Guards/                         GuardClauses, Ensure
│   ├─ Constants/                      GlobalConstants, RegexPatterns
│   ├─ Types/                          Result, Maybe, PaginatedList
│   └─ Utils/                          DateTimeProvider, Hashing, Crypto

├─ Features/                           ← CQRS Vertical Slices per Bounded-Context
│   ├─ Catalog/
│   │   ├─ Commands/                   AddItem, UpdatePrice
│   │   ├─ Queries/                    GetItemById, SearchItems
│   │   ├─ Events/                     ItemPriceChangedEvent
│   │   ├─ DTOs/                       ItemDto, ItemSummaryDto
│   │   ├─ Validators/                 AddItemValidator
│   │   ├─ Profiles/                   MappingProfile
│   │   ├─ HttpClients/                PricingHttpClient, InventoryHttpClient
│   │   └─ Jobs/                       ReindexCatalogJob
│   ├─ Orders/
│   │   ├─ Commands/                   CreateOrder, CancelOrder
│   │   ├─ Queries/                    GetOrderDetails, ListOrders
│   │   ├─ Events/                     OrderPlacedEvent, OrderCancelledEvent
│   │   ├─ DTOs/                       OrderDto, OrderLineDto
│   │   ├─ Validators/                 CreateOrderValidator
│   │   ├─ Profiles/                   OrderMappingProfile
│   │   ├─ HttpClients/                PaymentHttpClient
│   │   └─ Jobs/                       ExpireOrdersJob
│   └─ Identity/
│       ├─ Commands/                   RegisterUser, ChangePassword
│       ├─ Queries/                    GetUserProfile, ListRoles
│       ├─ Events/                     UserRegisteredEvent
│       ├─ DTOs/                       UserDto, RoleDto
│       ├─ Validators/                 RegisterUserValidator
│       ├─ Profiles/                   IdentityMappingProfile
│       └─ HttpClients/                OAuthHttpClient

├─ Services/                           ← Use-Case Facades
│   ├─ ApplicationServices/            OrderService, CatalogService
│   ├─ Workflow/                       OrderWorkflowManager, RefundWorkflow
│   └─ Coordinators/                   SagaCoordinator, TransactionCoordinator

├─ Messaging/                          ← Message Publishing/Consumption Layer
│   ├─ Publishers/                     EventPublisher, CommandPublisher
│   ├─ Consumers/                      EventConsumer, CommandConsumer
│   ├─ Subscribers/                    MessageBrokerSubscriptions
│   └─ IntegrationEvents/              Event Contract-level DTOs

├─ BackgroundJobs/                     ← Background Processing
│   ├─ Schedulers/                     QuartzScheduler, HangfireScheduler
│   ├─ JobDefinitions/                 CronExpressions, JobMetadata
│   ├─ JobHandlers/                    ExpireOrderHandler, SyncCatalogHandler
│   └─ Workers/                        BackgroundWorkerBase, HostedServices

├─ HttpClients/                        ← Typed Clients for External APIs
│   ├─ PaymentApiClient/
│   ├─ InventoryApiClient/
│   └─ NotificationApiClient/

├─ Mappings/                           ← Global AutoMapper Configuration
│   └─ RegisterProfiles.cs

├─ Validators/                         ← Shared FluentValidation RuleSets
│   └─ SharedRules.cs

├─ Tenancy/                            ← Multi-Tenant Support
│   ├─ Providers/                      HttpTenantProvider, BackgroundTenantProvider
│   ├─ Strategies/                     DatabasePerTenant, SchemaPerTenant
│   └─ TenantContext.cs

├─ Pagination/                         ← Global Pagination Models
│   ├─ PageRequest.cs
│   └─ PageResponse.cs

├─ Extensions/                         ← Helper Extension Methods
│   ├─ EnumerableExtensions.cs
│   ├─ MediatorExtensions.cs
│   └─ ServiceCollectionExtensions.cs

├─ ReadModels/                         ← Separate Query-side Models
│   ├─ OrderReadModel.cs
│   └─ CatalogReadModel.cs

├─ Resources/                          ← Localization Files, Error Messages
│   └─ *.resx

├─ Config/                             ← Strongly-typed Options
│   ├─ CacheSettings.cs
│   ├─ JwtSettings.cs
│   └─ EmailSettings.cs

├─ Diagnostics/                        ← Application-specific Trace/Logging
│   ├─ LogTemplates.cs
│   └─ TelemetryConstants.cs
```

## Layer Description

This Application layer implements the core business logic and orchestration of the enterprise system. It follows several key principles and patterns:

1. **Clean Architecture**: Strict separation of concerns and dependency inversion
2. **CQRS**: Command Query Responsibility Segregation for better scalability
3. **DDD**: Domain-Driven Design principles in bounded contexts
4. **Event-Driven**: Asynchronous communication through events and messages
5. **Cross-Cutting Concerns**: Centralized handling of common requirements

## Key Components

### Abstractions
Contains all interfaces defining the contracts for infrastructure services, following the Dependency Inversion Principle.

### Features
Vertical slices of functionality organized by business domain, each containing its complete CQRS implementation.

### Services
Application services that orchestrate use cases and workflow management.

### CrossCuttings
Handles cross-cutting concerns like validation, logging, and transaction management.

### Common
Shared utilities, types, and helper functions used across the application layer.

## Best Practices

1. Follow SOLID principles
2. Implement proper exception handling
3. Use strongly-typed configurations
4. Implement proper logging and telemetry
5. Follow consistent naming conventions
6. Document public APIs and important implementation details
7. Write unit tests for business logic
8. Use validation for all inputs
9. Implement proper security measures

## Getting Started

1. Review the directory structure
2. Identify the bounded context for your feature
3. Implement in the appropriate Features/ subdirectory
4. Follow existing patterns and conventions
5. Add appropriate tests
6. Document any new patterns or significant changes

## Contributing

1. Follow the established directory structure
2. Maintain separation of concerns
3. Document new features and significant changes
4. Add unit tests for new functionality
5. Review existing patterns before implementing new ones
6. Keep cross-cutting concerns centralized
7. Maintain backward compatibility when possible 