# Architecture

## Overview

The system follows a clean architecture approach with clear separation between domain logic, application orchestration, infrastructure concerns, and entry points. Dependencies flow inward — outer layers depend on inner layers, never the reverse.

```
┌─────────────────────────────────────────┐
│           Ticketing.API                 │  HTTP entry point
│           Ticketing.Worker              │  Background jobs entry point
├─────────────────────────────────────────┤
│         Ticketing.Application           │  Use cases, handlers, interfaces
├─────────────────────────────────────────┤
│           Ticketing.Domain              │  Aggregates, entities, events
├─────────────────────────────────────────┤
│        Ticketing.Infrastructure         │  EF Core, Redis, repositories
├─────────────────────────────────────────┤
│         Ticketing.Contracts             │  Shared DTOs and request/response types
└─────────────────────────────────────────┘
```

---

## Projects

### Ticketing.Domain

The core of the system. Contains aggregates, entities, value objects, domain events, and domain exceptions. Has no dependencies on any other project or framework.

Key types: `Reservation`, `Payment`, `Screening`, `ScreeningSeat`, `Hall`, `Seat`, `Money`

See [domain-model.md](./domain-model.md) for full details.

---

### Ticketing.Application

Orchestrates use cases. Contains command/query handlers (CQRS), domain event handlers, outbox processing logic, and interface definitions that infrastructure must implement.

Depends on: `Ticketing.Domain`

Key responsibilities:

- Command and query handlers via MediatR
- `IEventHandler<T>` implementations for domain event processing
- `OutboxProcessorService` — processes and dispatches outbox messages
- Interface definitions: `IUnitOfWork`, `IOutboxRepository`, `IEventDispatcher`, `ISeatLockService`, repository interfaces

See [cqrs.md](./cqrs.md) and [outbox-pattern.md](./outbox-pattern.md) for details.

---

### Ticketing.Infrastructure

Implements the interfaces defined in `Ticketing.Application`. Contains all external concerns — database access, Redis, and repository implementations.

Depends on: `Ticketing.Application`, `Ticketing.Domain`

Key responsibilities:

- EF Core `DbContext` with outbox message interception in `SaveChangesAsync`
- Repository implementations
- `RedisSeatLockService` — distributed seat locking
- `EventDispatcher` — resolves and invokes event handlers by type name

Registered via `AddInfrastructure(configuration)` extension method.

See [redis-locking.md](./redis-locking.md) for locking details.

---

### Ticketing.API

The HTTP entry point. Hosts controllers and the request pipeline.

Depends on: `Ticketing.Application`, `Ticketing.Infrastructure`

Registered services:

- MVC controllers
- Swagger / OpenAPI
- Application and infrastructure layers via extension methods
- CORS policy for local frontend development (`http://localhost:4200`)

```
Request → Controller → MediatR Command → Handler → Domain → SaveChangesAsync → Outbox
```

---

### Ticketing.Worker

A background `IHost` that runs long-lived background services. Shares the same application and infrastructure registration as the API but has no HTTP pipeline.

Depends on: `Ticketing.Application`, `Ticketing.Infrastructure`

Hosted services:

| Service                       | Responsibility                                           |
| ----------------------------- | -------------------------------------------------------- |
| `OutboxProcessor`             | Polls outbox every 200ms, dispatches pending events      |
| `ReservationExpirationWorker` | Expires pending reservations past their `ExpiredAt` time |

See [outbox-pattern.md](./outbox-pattern.md) for outbox processing details.

---

### Ticketing.Contracts

Shared request and response types used across the API boundary. Keeps DTOs out of the domain and application layers.

---

## Dependency Graph

```
Ticketing.API ──────────────┐
                            ├──► Ticketing.Application ──► Ticketing.Domain
Ticketing.Worker ───────────┘

Ticketing.Infrastructure ───► Ticketing.Application
                         ───► Ticketing.Domain

Ticketing.Contracts ◄─── Ticketing.API
```

---

## Key Design Decisions

**Domain has no framework dependencies** — `Ticketing.Domain` references nothing outside the .NET BCL. Aggregates raise domain events internally; infrastructure intercepts them in `SaveChangesAsync`.

**Two entry points, one application core** — `Ticketing.API` and `Ticketing.Worker` both register the same `AddApplication()` and `AddInfrastructure()` services. The worker runs the outbox and expiration logic that the API triggers indirectly through domain events.

**Outbox over direct dispatch** — domain events are never dispatched inline during a request. They are persisted to the database in the same transaction as the state change, then processed asynchronously by the worker. This decouples the HTTP response time from event handling and provides at-least-once delivery.

**Redis for contention, database for correctness** — seat locking uses Redis as a fast gate to reduce database contention, with EF Core optimistic concurrency as the final consistency guarantee. See [concurrency.md](./concurrency.md).
