# Redis Locking

## Why

Seat reservation is a high-contention operation — multiple users can attempt to book the same seat simultaneously. Hitting the database directly for every concurrent request creates unnecessary load and relies solely on the database to reject conflicts.

Redis distributed locks act as a fast gate: they reject concurrent reservation attempts immediately, before any database interaction, keeping the write path clean under load.

See [concurrency.md](./concurrency.md) for how Redis locking fits into the full two-layer concurrency model.

## Lock Design

Each seat in a screening gets its own lock key:

```
seat:{screeningId}:{seatId}
```

This allows fine-grained locking per seat rather than locking the entire screening.

## Acquiring a Lock

```csharp
var key = $"seat:{screeningId}:{seatId}";
var value = Guid.NewGuid().ToString();

var acquired = await db.StringSetAsync(
    key,
    value,
    TimeSpan.FromSeconds(10),
    When.NotExists);

return acquired ? value : null;
```

Three properties make this safe:

- **`NX` (set if not exists)** — the Redis `SET NX` command is atomic. Only one caller can set the key; all others get `false` immediately.
- **Unique token (GUID)** — each lock attempt generates a unique value. This is stored alongside the key and is required to release the lock.
- **TTL: 10 seconds** — the lock auto-expires if the process crashes before releasing it, preventing a permanently stuck seat.

Returns the lock token on success, `null` if the seat is already locked.

## Releasing a Lock

```csharp
const string script = @"
    if redis.call('GET', KEYS[1]) == ARGV[1] then
        return redis.call('DEL', KEYS[1])
    else
        return 0
    end";

await db.ScriptEvaluateAsync(
    script,
    new RedisKey[] { key },
    new RedisValue[] { lockValue });
```

Release uses a Lua script executed atomically on the Redis server. The script:

1. Reads the current value of the key
2. Compares it to the token provided by the caller
3. Deletes the key only if they match

This guarantees that **only the lock owner can release the lock**. Without this check, a slow request whose TTL expired could delete a lock acquired by a different request, opening a window for a double-booking.

## All-or-Nothing Acquisition

When reserving multiple seats, all seats must be locked before proceeding. If any lock fails, all already-acquired locks are released immediately:

```csharp
foreach (var seatId in request.SeatIds)
{
    var lockValue = await _seatLockService.TryLockSeatAsync(screening.Id, seatId);

    if (lockValue == null)
    {
        foreach (var kv in lockedSeats)
            await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);

        return Result.Failure("Seat is already being reserved");
    }

    lockedSeats[seatId] = lockValue;
}
```

This prevents partial locks — a request never holds some seats locked while failing to acquire others.

Locks are always released in a `finally` block after the database write completes, regardless of success or failure.

## Failure Modes

| Scenario                               | Behaviour                                                         |
| -------------------------------------- | ----------------------------------------------------------------- |
| Seat already locked by another request | Returns `null` immediately, caller gets 409                       |
| Process crashes before release         | TTL expires after 10 seconds, seat becomes lockable again         |
| Slow request whose TTL expired         | Lua script prevents it from deleting another request's lock       |
| Redis unavailable                      | Exception propagates, reservation fails safely — no partial state |

## What Redis Does Not Guarantee

Redis is not the source of truth for seat availability. It is a performance optimization that fails fast under contention. The database remains the correctness guarantee via EF Core optimistic concurrency (`[Timestamp]`).

Removing Redis would still produce correct results under the database layer alone — just with higher DB contention. Removing the database concurrency token while keeping Redis would be unsafe.
