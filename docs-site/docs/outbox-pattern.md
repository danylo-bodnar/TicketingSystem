# Outbox Pattern

## Why

Domain events raised during a request (e.g. `ReservationCreated`, `PaymentCompleted`) must be delivered reliably — even if the process crashes immediately after saving to the database. Publishing directly to a queue or dispatcher inside the request handler means a crash between "save" and "publish" loses the event silently.

The Outbox pattern solves this by persisting events **in the same database transaction** as the domain state change, then dispatching them asynchronously from a background worker.

---

## How It Works

```
HTTP Request
    │
    ▼
Command Handler
    │  raises domain event
    ▼
SaveChangesAsync()
    ├── persists aggregate state
    └── persists OutboxMessage        ← same transaction

Background Worker (every 200ms)
    │
    ▼
OutboxProcessorService.ProcessOnce()
    ├── fetch pending messages
    ├── dispatch each via EventDispatcher
    └── mark as processed (or increment retry)
```

The critical guarantee: **if the DB write succeeds, the event will eventually be dispatched.** The two are atomic.

---

## Components

### 1. OutboxMessage

Persisted alongside aggregate state in `SaveChangesAsync`:

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }       // e.g. "ReservationCreated"
    public string Payload { get; set; }    // JSON-serialized event
    public int RetryCount { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public DateTime? DeadLetteredAt { get; set; }
    public string? LastError { get; set; }
}
```

### 2. DbContext — SaveChangesAsync override

Domain events are intercepted before the actual save, serialized, and written as `OutboxMessage` rows in the same transaction:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var domainEvents = ChangeTracker
        .Entries<AggregateRoot>()
        .SelectMany(e => e.Entity.DomainEvents)
        .ToList();

    foreach (var domainEvent in domainEvents)
    {
        OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent),
            OccurredAt = DateTime.UtcNow
        });
    }

    var result = await base.SaveChangesAsync(ct);

    foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
        entry.Entity.ClearDomainEvents();

    return result;
}
```

### 3. OutboxProcessorService

Fetches pending messages and dispatches them. On failure, applies exponential backoff. After 5 retries, marks as dead-lettered:

```csharp
public async Task ProcessOnce(CancellationToken ct)
{
    var messages = await _outbox.GetPendingAsync(20, ct);

    foreach (var msg in messages)
    {
        try
        {
            await _dispatcher.DispatchAsync(msg.Type, msg.Payload, ct);
            msg.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            msg.RetryCount++;
            msg.LastError = ex.Message;

            if (msg.RetryCount >= 5)
                msg.DeadLetteredAt = DateTime.UtcNow;
            else
                msg.NextRetryAt = DateTime.UtcNow.AddSeconds(5 * msg.RetryCount);
        }
    }

    await _unitOfWork.SaveChangesAsync(ct);
}
```

Retry backoff schedule:

| Attempt | Delay         |
| ------- | ------------- |
| 1       | 5s            |
| 2       | 10s           |
| 3       | 15s           |
| 4       | 20s           |
| 5       | Dead-lettered |

### 4. OutboxProcessor (Background Worker)

Runs `ProcessOnce` every 200ms as a hosted `BackgroundService`:

```csharp
protected override async Task ExecuteAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        using var scope = _scopeFactory.CreateScope();
        var processor = scope.ServiceProvider
            .GetRequiredService<OutboxProcessorService>();

        await processor.ProcessOnce(ct);
        await Task.Delay(200, ct);
    }
}
```

A new DI scope is created per iteration so scoped services (DbContext, repositories) are resolved correctly.

### 5. EventDispatcher

Resolves the event type by name, deserializes the payload, and invokes all registered `IEventHandler<T>` implementations:

```csharp
public async Task DispatchAsync(string typeName, string payload, CancellationToken ct)
{
    var type = EventTypeResolver.Resolve(typeName);
    var @event = JsonSerializer.Deserialize(payload, type);

    var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
    var handlers = _provider.GetServices(handlerType);

    foreach (var handler in handlers)
    {
        var method = handlerType.GetMethod("HandleAsync");
        await (Task)method.Invoke(handler, [@event, ct])!;
    }
}
```

---

## Event Chain

A single command can trigger a chain of events, each handled independently through the outbox:

```
CompletePaymentCommand
    └── payment.Complete()
        └── PaymentCompleted (outbox)
            └── reservation.Confirm()
                └── ReservationConfirmed (outbox)
                    └── seat.MarkAsSold()
```

Each step is persisted and retried independently. If a step fails transiently, only that step is retried — not the entire chain.

---

## Dead Letters

Messages that fail 5 consecutive times are marked with `DeadLetteredAt` and stopped from retrying. These represent either a persistent infrastructure failure or a bug.

---

## Delivery Guarantee

The outbox provides **at-least-once delivery** — a message may be dispatched more than once if the worker crashes after dispatching but before marking it as processed. Event handlers should be idempotent where possible.
