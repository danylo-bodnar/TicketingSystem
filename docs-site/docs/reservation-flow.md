## `CreateReservationHandler`

### Purpose

Handles creation of a seat reservation for a given screening.

This is a high-contention write operation that must ensure:
- correct seat state transitions
- protection against concurrent reservations
- transactional consistency
- safe cleanup in all scenario
## Core Idea

Seat reservation is a contention-heavy operation.

To ensure both performance and correctness, the system uses:

- **Redis distributed locks** to prevent concurrent reservation attempts early
- **Database optimistic concurrency** as the final consistency guarantee

Redis reduces contention, while the database remains the source of truth.

## Execution Flow

### 1. Input Validation

Ensures at least one seat is selected.

**Failure:** returns `"No seats selected"`.

### 2. Load Screening Aggregate

Fetches the screening by `ScreeningId`.

**Failure:** returns `"Screening not found"`.

### 3. Concurrency Strategy

The system uses a two-layer concurrency model:

1. **Redis locking** prevents concurrent reservation attempts at runtime

2. **Optimistic concurrency (DB)** guarantees correctness at commit time

##### Redis Locking (runtime protection)

Each seat is locked using Redis (`SET NX EX`):

```csharp
var key = $"seat:{screeningId}:{seatId}";
var value = Guid.NewGuid().ToString();

var acquired = await db.StringSetAsync(
    key,
    value,
    TimeSpan.FromSeconds(10),
    When.NotExists);
```

- If any lock fails, all previously acquired locks are released
- A unique lock value (GUID) ensures only the lock owner can release it

##### Optimistic Concurrency (persistence protection)

Each `ScreeningSeat` uses a row version:

```csharp
[Timestamp]
public uint Version { get; private set; }
```

If a concurrent update occurs, `SaveChangesAsync` fails with a concurrency exception.

This ensures correctness even if a Redis lock expires mid-request multiple instances race under load.

### 4. Domain Mutation

Seats are reserved through the aggregate:

```csharp
public void Reserve()
{
    if (Status != ScreeningSeatStatus.Available)
        throw new ScreeningSeatNotAvailableException(Id);

    Status = ScreeningSeatStatus.Reserved;
}
```

Seat transitions:

```
Available → Reserved → Sold
```

A seat can only be reserved if its status is `Available`.

### 5. Persist Reservation

A new reservation is created and saved:

```csharp
var reservation = new Reservation(
    Guid.NewGuid(),
    screening.EventId,
    screening.Id,
    [.. lockedSeats.Keys]);

await _reservations.AddAsync(reservation);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Both seat updates and reservation creation are committed atomically.

**Failure:** concurrency exception if state changed during processing.

### 6. Lock Cleanup

Locks are always released in a `finally` block:

```csharp
finally
{
    foreach (var kv in lockedSeats)
        await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);
}
```

Release uses a compare-and-delete Lua script:

```lua
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end
```

- ensures only the lock owner can release it
- prevents deleting a lock acquired by another request

All locks have a **10-second TTL** as a fallback in case of crashes.

## Error Reference

| Condition            | Result                                                |
| -------------------- | ----------------------------------------------------- |
| No seats in request  | `"No seats selected"`                                 |
| Screening not found  | `"Screening not found"`                               |
| Seat already locked  | `"Seat is already being reserved"`                    |
| Seat does not exist  | `"One or more seats do not exist in this screening."` |
| Seat not available   | `"One or more seats are no longer available."`        |
| Concurrency conflict | `"Seats are no longer available."`                    |

## Concurrency Guarantees

| Threat                           | Mitigation                                |
| -------------------------------- | ----------------------------------------- |
| Concurrent reservation attempts  | Redis distributed lock (`SET NX`)         |
| Lock expiry during processing    | DB optimistic concurrency (`[Timestamp]`) |
| Process crash before cleanup     | Redis TTL (10s auto-expiry)               |
| Releasing another request’s lock | Lua compare-and-delete                    |
