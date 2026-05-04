# Concurrency

## The Problem

A ticketing system is a classic concurrency target — multiple users can attempt to book the same seat at the same time. Without protection, two requests could both read a seat as `Available`, both pass the availability check, and both succeed, resulting in an oversold seat.

This system uses two complementary layers to prevent this.

## Layer 1 — Redis Distributed Lock (fail fast)

When a reservation request arrives, before touching the database, each requested seat is locked in Redis:

```
POST /api/Reservations
    │
    ├── TryLockSeatAsync(screeningId, seatId)
    │       └── SET lock:screening:{id}:seat:{id} {token} NX EX 10
    │
    ├── if lock fails → release all acquired locks → return 409
    │
    └── if all locks acquired → proceed to DB write
```

Key properties of the lock:

- **NX (set if not exists)** — atomic, only one caller wins
- **TTL: 10 seconds** — lock auto-expires if the process crashes before releasing
- **Unique token per lock** — prevents a caller from releasing a lock it doesn't own
- **All-or-nothing** — if any seat in the request fails to lock, all already-acquired locks are released immediately

```csharp
var lockedSeats = new Dictionary<Guid, string>();

foreach (var seatId in request.SeatIds)
{
    var lockValue = await _seatLockService.TryLockSeatAsync(screening.Id, seatId);

    if (lockValue == null)
    {
        // Release everything acquired so far
        foreach (var kv in lockedSeats)
            await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);

        return Result.Failure("Seat is already being reserved");
    }

    lockedSeats[seatId] = lockValue;
}
```

Locks are always released in a `finally` block after the DB write completes.

### What Redis protects against

49 out of 50 concurrent requests for the same seat are rejected immediately at the Redis layer, before any database load. This keeps the DB write path clean and fast under contention.

### What Redis does not guarantee

Redis is not a substitute for database-level consistency. A Redis node restart, network partition, or lock expiry during a slow DB write could theoretically allow two requests through simultaneously. This is where Layer 2 takes over.

## Layer 2 — Optimistic Concurrency (correctness guarantee)

The `ScreeningSeat` entity carries an EF Core `[Timestamp]` concurrency token:

```csharp
public class ScreeningSeat
{
    public ScreeningSeatStatus Status { get; private set; }

    [Timestamp]
    public uint Version { get; private set; }
}
```

When a seat is updated, EF Core includes the original `Version` value in the `WHERE` clause:

```sql
UPDATE ScreeningSeats
SET Status = 'Reserved', ...
WHERE SeatId = @seatId AND Version = @originalVersion
```

If another request has already updated the row, `Version` will have changed and the update affects 0 rows. EF Core detects this and throws a `DbUpdateConcurrencyException`, which is caught and returned as a failure:

```csharp
catch (ConcurrencyException)
{
    return Result.Failure("Seats are no longer available.");
}
```

### What optimistic concurrency protects against

Any race condition that slips past the Redis layer. It is the hard consistency guarantee — no matter what happens at the application layer, the database will never allow two reservations for the same seat.

## Why Both Layers

|                              | Redis Lock                      | Optimistic Concurrency              |
| ---------------------------- | ------------------------------- | ----------------------------------- |
| **Purpose**                  | Fail fast, reduce DB contention | Correctness guarantee               |
| **Where**                    | Application layer               | Database layer                      |
| **Cost of failure**          | 409 returned immediately        | `DbUpdateConcurrencyException`      |
| **Handles**                  | Concurrent requests             | Redis misses, crashes, expiry races |
| **Required for correctness** | No                              | Yes                                 |

Redis is a performance optimization — it prevents the database from being hammered under contention. Optimistic concurrency is the safety net that ensures correctness even if Redis fails.

Removing Redis would still produce correct results, just with more DB load under concurrency. Removing the `[Timestamp]` token while keeping Redis would be unsafe.

## Concurrency Test

The system is verified under load with 50 concurrent requests for the same seat:

```csharp
[Fact]
public async Task OnlyOneReservation_ShouldSucceed_ForSameSeat_UnderConcurrency()
{
    var tasks = Enumerable.Range(0, 50).Select(_ =>
        Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = DataSeeder.Screening1Id,
            seatIds = new[] { seatId }
        })
    );

    var results = await Task.WhenAll(tasks);

    Assert.Equal(1, results.Count(r => r.IsSuccessStatusCode));

    // Verify DB state — not just HTTP responses
    Assert.Single(await verifyDb.Reservations.ToListAsync());
}
```

Expected outcome: exactly 1 success, 49 failures, 1 reservation row in the database.
