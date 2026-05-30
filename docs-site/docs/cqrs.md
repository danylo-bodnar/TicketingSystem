# CQRS

## Overview

The application layer uses **CQRS** (Command Query Responsibility Segregation) via **MediatR** to separate writes from reads.

- **Commands** — change state, return `Result<T>` or void
- **Queries** — read state, return `Result<T>`

Both are `IRequest<TResponse>` MediatR messages dispatched via `IMediator.Send()` from controllers.

## Commands

| Command | Signature | Handler |
|---|---|---|
| `CreateReservationCommand` | `IRequest<Result<CreateReservationResponse>>` | `CreateReservationHandler` |
| `CompletePaymentCommand` | `IRequest` (void) | `CompletePaymentHandler` |
| `FailPaymentCommand` | `IRequest` (void) | `FailPaymentHandler` |

## Queries

| Query | Signature | Handler |
|---|---|---|
| `GetAllScreeningsQuery` | `IRequest<Result<List<ScreeningResponse>>>` | `GetAllScreeningsHandler` |
| `GetAvailableSeatsQuery` | `IRequest<Result<List<SeatDto>>>` | `GetAvailableSeatsHandler` |
| `GetScreeningSeatsQuery` | `IRequest<Result<List<SeatDto>>>` | `GetScreeningSeatsHandler` |

## Handlers

Each handler implements `IRequestHandler<TRequest, TResponse>` and contains exactly one `Handle` method. Handlers are registered automatically via assembly scanning in `AddApplication()`.

```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
```

### Handler conventions

- Inject domain repositories and infrastructure interfaces, never `IMediator` or other handlers
- Return `Result<T>.Success(...)` or `Result<T>.Failure("reason")` for queries and data-returning commands
- For void commands (`IRequest`), throw on failure — the exception bubbles up to the `GlobalExceptionHandler`
- Log domain-specific outcomes (entity created, not found, etc.) — envelope logging (start/completion/duration) is handled by the pipeline behavior

## Result\<T\> Pattern

Every handler that returns data returns `Result<T>`:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Value { get; }

    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure);
}
```

Controllers map results to HTTP responses via `HandleResult()` (for direct OK/BadRequest mapping) or `Match()` (for custom responses like 201 Created):

```csharp
// Simple mapping
var result = await _mediator.Send(new GetAvailableSeatsQuery(id));
return HandleResult(result);

// Custom mapping
var result = await _mediator.Send(command);
return result.Match(
    success => HandleCreatedResult(result, nameof(CreateReservation), new { id = success.ReservationId }),
    failure => HandleFailure(result)
);
```

## Pipeline Behaviors

Behaviors wrap every command/query execution, enabling cross-cutting concerns without touching individual handlers.

### LoggingBehavior

Registered as an open generic `IPipelineBehavior<,>`:

```csharp
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

Logs every request automatically:

| Event | Level | Example |
|---|---|---|
| Request started | Debug | `Processing CreateReservationCommand` |
| Completed successfully | Information | `Completed CreateReservationCommand in 45ms` |
| Returned `Result.Failure` | Warning | `Completed GetAvailableSeatsQuery in 12ms — Failed: Screening not found` |
| Unhandled exception | Error | `Exception in CreateReservationCommand after 30ms: ...` |

See [logging.md](./logging.md) for full details.

### Future behaviors

Validation, performance thresholds, and audit logging fit the same pattern — add a class implementing `IPipelineBehavior<TRequest, TResponse>`, register in DI, and it runs automatically.

## Request Flow

```
HTTP Request
    │
    ▼
Controller
    ├── builds command/query from route + body
    ├── _mediator.Send(request)
    │       │
    │       ▼
    │   MediatR pipeline
    │       │
    │       ├── LoggingBehavior (start timer)
    │       │       │
    │       │       ▼
    │       │   Handler.Handle()
    │       │       │
    │       │       ├── Repository calls
    │       │       ├── Domain operations
    │       │       └── SaveChangesAsync → outbox
    │       │       │
    │       │       ▼
    │       ├── LoggingBehavior (stop timer, log success/failure)
    │       │
    │       ▼
    ├── Result<T> returned to controller
    │
    ▼
HTTP Response
```

## Registration

All CQRS infrastructure is set up in `Ticketing.Application.DependencyInjection.AddApplication()`:

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

    // Domain event handling, background services, etc.

    return services;
}
```
