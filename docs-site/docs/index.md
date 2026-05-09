# Ticketing System

A backend system for high-concurrency ticket booking, built as a pet project to explore distributed systems patterns in a realistic domain.

The system handles the full reservation lifecycle — from seat selection through payment confirmation to inventory update — while staying correct under concurrent load. The core challenge is that multiple users can attempt to book the same seat simultaneously, and the system must guarantee that only one succeeds without sacrificing performance.

## Architecture Approach

The system is built around **Domain-Driven Design** principles. Business logic lives in the domain layer — aggregates like `Reservation`, `Payment`, and `Screening` own their state transitions and enforce their own invariants. Nothing outside the domain can put an aggregate into an invalid state.

The application layer orchestrates use cases through **CQRS** — commands mutate state, queries read it, and they are handled independently via MediatR. Infrastructure concerns like persistence, caching, and messaging are kept behind interfaces defined by the application layer.

## Key Patterns

**Outbox Pattern** — domain events are persisted to the database in the same transaction as the state change, then dispatched asynchronously by a background worker. This guarantees at-least-once delivery without coupling the HTTP response to event processing.

**Two-layer concurrency** — seat reservation uses Redis distributed locks to fail fast under contention, backed by EF Core optimistic concurrency as the correctness guarantee at the database level. The two layers are complementary: Redis is a performance optimization, the database is the source of truth.

**Event-driven flow** — the reservation, payment, and seat-sold chain is driven entirely by domain events. No handler calls another handler directly — each step completes, raises an event, and the next step is triggered asynchronously through the outbox.

## Stack

- **ASP.NET Core** — HTTP API
- **PostgreSQL** — primary database
- **Redis** — distributed seat locking
- **EF Core** — ORM with optimistic concurrency
- **MediatR** — CQRS command/query dispatching
- **Serilog** — structured logging with correlation ID tracing
- **xUnit** — unit and integration tests against a real database

## Docs

- [Architecture](./architecture.md)
- [CQRS](./cqrs.md)
- [Concurrency](./concurrency.md)
- [Domain Model](./domain-model.md)
- [Messaging](./messaging.md)
- [Outbox Pattern](./outbox-pattern.md)
- [Redis Locking](./redis-locking.md)
- [Testing](./testing.md)
