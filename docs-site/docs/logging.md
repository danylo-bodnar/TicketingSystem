# Logging

## What We Use

**Serilog** with structured logging. Instead of plain text strings, every log entry is a structured object with named properties that can be queried individually.

Plain text logging:

```
[INFO] Reservation 3f2a created for screening 9b1c
```

Structured logging:

```json
{
  "timestamp": "2026-05-06T10:23:11Z",
  "level": "Information",
  "message": "Reservation created",
  "ReservationId": "3f2a...",
  "ScreeningId": "9b1c...",
  "CorrelationId": "7d4e...",
  "SeatCount": 2
}
```

The difference is that fields are queryable — in a log viewer you can filter by `ReservationId` and see every log entry related to that reservation across both the API and the Worker.

## CorrelationId

Every HTTP request is assigned a `CorrelationId` that flows through the entire system — from the initial API request through the outbox into the background worker.

```
HTTP Request
    │
    ▼
CorrelationIdMiddleware
    ├── reads X-Correlation-Id header (or generates a new GUID)
    ├── writes it back to the response header
    ├── sets CorrelationContext.CorrelationId (scoped service)
    └── pushes it into Serilog LogContext
         │
         ▼
    Command Handler → SaveChangesAsync
         └── OutboxMessage.CorrelationId = CorrelationContext.CorrelationId
                                                │
                                                ▼
                                        OutboxProcessorService (Worker)
                                            └── LogContext.PushProperty("CorrelationId", msg.CorrelationId)
                                                 │
                                                 ▼
                                            Event Handlers
                                                └── all logs automatically include CorrelationId
```

This means you can take the `X-Correlation-Id` from any API response and find every log entry related to that request — including what the worker did minutes later.

### CorrelationContext

A scoped service that carries the correlationId through the request:

```csharp
public class CorrelationContext
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
```

Set by middleware, injected into `DbContext` to stamp outbox messages. Scoped to the request lifetime so each request gets its own instance.

## Where We Log

### Application layer — yes

This is where business operations happen and where failures matter. All handlers log here:

- **MediatR pipeline** — logs every command/query start, completion, duration, and failure automatically
- **Command/event handlers** — supplement with domain-specific details (entity IDs, reasons, counts)
- **Outbox and event dispatcher** — message-level logging

### Query handlers

Previously opted out of logging. Now covered automatically by the [MediatR logging pipeline](#mediatr-logging-pipeline) — every query logs start time, duration, and success/failure without any handler changes.

### Infrastructure layer — no

EF Core and Redis driver logs cover normal operations. We only let infrastructure exceptions bubble up and get logged by the application layer.

### Domain layer — never

Domain has no logging dependency and should not have one.

## What We Log

The rule: **log decisions and outcomes, not mechanics.**

| ✅ Log this                             | ❌ Not this                  |
| --------------------------------------- | ---------------------------- |
| "Reservation created"                   | "Called SaveChangesAsync"    |
| "Payment completed for reservation X"   | "Called StringSetAsync"      |
| "Seat already locked"                   | "Acquired Redis connection"  |
| "Message dead-lettered after 5 retries" | "Fetched 20 outbox messages" |

### Log levels

| Level         | When                                                                 |
| ------------- | -------------------------------------------------------------------- |
| `Information` | Successful business operation completed                              |
| `Warning`     | Expected failure — seat locked, not found, duplicate ignored         |
| `Error`       | Unexpected failure — exception, dead-letter, deserialization failure |

## Logged Components

### MediatR Logging Pipeline

`LoggingBehavior<TRequest, TResponse>` wraps every command and query automatically:

| Event | Level | Example |
|---|---|---|
| Request started | Debug | `Processing CreateReservationCommand` |
| Completed successfully | Information | `Completed CreateReservationCommand in 45ms` |
| Returned `Result.Failure` | Warning | `Completed GetAvailableSeatsQuery in 12ms — Failed: Screening not found` |
| Unhandled exception | Error | `Exception in CreateReservationCommand after 30ms: ...` |

The pipeline is a single `IPipelineBehavior` registered in `DependencyInjection.cs`. It is the **envelope** — provides timing, type name, and outcome. Handlers provide the **domain story** beneath it.

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### CorrelationIdMiddleware

- Logs nothing directly — pushes `CorrelationId` into `LogContext` for all downstream logs

### OutboxProcessorService

- Message processed successfully
- Message failed with retry scheduled (Warning + exception)
- Message dead-lettered after 5 retries (Error + exception)

### EventDispatcher

- Event type being dispatched and handler count
- Dispatch completed successfully
- Deserialization failure (Error)

### Command handlers

- `CreateReservationHandler` — reservation created, seat lock failures, concurrency conflicts
- `CompletePaymentHandler` — payment completed
- `FailPaymentHandler` — payment failed

### Event handlers

- `ReservationCreatedHandler` — payment created with amount
- `ReservationConfirmedHandler` — seats marked as sold with count
- `PaymentCompletedHandler` — reservation confirmed

## Log Output

All logs include `CorrelationId` automatically via `LogContext`. The outbox worker additionally includes `MessageType` and `MessageId` per message:

```json
{
  "timestamp": "2026-05-06T10:23:15Z",
  "level": "Information",
  "message": "Outbox message processed",
  "CorrelationId": "7d4e-...",
  "MessageType": "PaymentCompleted",
  "MessageId": "a1b2-...",
  "SourceContext": "Ticketing.Application.Outbox.OutboxProcessorService"
}
```

## Sinks

| Environment | Sink          |
| ----------- | ------------- |
| Development | Console       |
| Production  | Console + Seq |

Seq provides a UI for querying structured logs by any property. See [architecture.md](./architecture.md) for deployment details.

Noisy framework logs are suppressed in config:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

Without this, EF Core query logs and ASP.NET request pipeline logs dominate the output.
