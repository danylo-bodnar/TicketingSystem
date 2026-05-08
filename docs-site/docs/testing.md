# Testing

## Overview

The test suite is split into two projects — unit tests for domain logic and integration tests for the full application stack including the database, outbox, and HTTP pipeline.

## Unit Tests

Located in `Ticketing.UnitTests`. Tests domain aggregates and entities in isolation with no infrastructure dependencies.

### What is tested

**Reservation** — state transition rules:

- `Confirm()` sets status to `Confirmed` when `Pending`
- `Confirm()` throws when already `Confirmed`
- `Cancel()` and `Expire()` set correct status from `Pending`

**Payment** — state transition rules:

- `Complete()` sets status to `Completed` when `Pending`
- `Complete()` throws when already `Completed`

**ScreeningSeat** — seat lifecycle:

- `Reserve()` transitions from `Available` to `Reserved`
- `Reserve()` throws when already `Reserved` or `Sold`
- `MarkAsSold()` transitions from `Reserved` to `Sold`
- `MarkAsSold()` throws when not `Reserved`
- `Release()` transitions back to `Available`
- `Release()` throws when already `Sold`

**Screening** — aggregate invariants:

- Cannot be created without seats
- `GetSeat()` returns correct seat by ID
- `GetSeat()` throws when seat not found

**Hall** — aggregate invariants:

- Cannot be created without seats
- `AddSeat()` succeeds for valid seats
- `AddSeat()` throws on duplicate row/column
- `AddSeat()` throws when seat belongs to another hall
- `GetSeat()` returns correct seat, throws when not found

**Seat** — construction invariants:

- Row cannot be empty or whitespace
- Column must be greater than 0

### Running unit tests

```bash
dotnet test Ticketing.UnitTests
```

## Integration Tests

Located in `Ticketing.IntegrationTests`. Tests the full application stack against a real PostgreSQL database using `WebApplicationFactory`.

### Infrastructure

**`CustomWebApplicationFactory`** — replaces the production `DbContext` registration with a test database connection:

```
Host=localhost;Port=5433;Database=ticketing_test
```

**`TestDatabaseFixture`** — shared across all tests in the `Integration` collection. On startup it runs `EnsureDeleted` + `Migrate` to guarantee a clean schema. Between each test `ResetDatabase()` truncates all tables and re-seeds test data.

**`IntegrationTestBase`** — base class for all integration tests. Provides:

- `Client` — `HttpClient` pointed at the test server
- `CreateDbContext()` — fresh `DbContext` for assertions
- `RunOutboxProcessorOnce()` — manually triggers the outbox processor
- `GetService<T>()` — resolves any registered service from DI
- `WaitForAsync()` — polls a condition with timeout, running the outbox processor on each iteration

```csharp
public async Task WaitForAsync(Func<Task<bool>> condition, int timeoutMs = 5000)
{
    var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
    while (DateTime.UtcNow < deadline)
    {
        if (await condition()) return;
        await RunOutboxProcessorOnce();
        await Task.Delay(100);
    }
    throw new TimeoutException("Condition not met within timeout");
}
```

This is the key utility for testing async event chains — it drives the outbox manually rather than relying on the background worker timer.

### Integration test coverage

**Outbox correctness:**

- Creating a reservation writes an outbox message with the correct type and payload
- A failed reservation writes no outbox message
- Running the outbox processor creates a payment from a `ReservationCreated` message

**Idempotency:**

- Handling `ReservationCreated` twice for the same reservation creates only one payment (verified via `DuplicateEntityException` handling)

**Duplicate reservation:**

- A second reservation attempt for the same seat fails and produces no outbox message

**Validation:**

- Empty seat list returns a failure response
- Duplicate seat IDs in the same request returns a failure response

**Concurrency:**

- 50 concurrent reservation attempts for the same seat result in exactly 1 success, 1 reservation row, and 1 outbox message

**Full end-to-end flow:**

```
POST /api/Reservations
    → outbox: ReservationCreated → Payment created (Pending)
    → POST /api/payments/webhook (completed)
    → outbox: PaymentCompleted → Reservation confirmed
    → outbox: ReservationConfirmed → Seats marked as Sold
```

Asserts final state: `Reservation = Confirmed`, `Seat = Sold`, `Payment = Completed`, no dead-lettered messages.

### Running integration tests

Requires a running PostgreSQL instance on port `5433`. Start it with Docker:

```bash
docker run -d --name ticketing-test-db \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=ticketing_test \
  -p 5433:5432 postgres:16
```

Then:

```bash
dotnet test Ticketing.IntegrationTests
```

## Test Design Decisions

**Manual outbox driving** — the background worker polls every 200ms which makes tests slow and timing-dependent. `RunOutboxProcessorOnce()` and `WaitForAsync()` give deterministic control over when events are processed.

**Per-test DB reset** — each test gets a clean slate via `TRUNCATE ... CASCADE` + re-seed rather than transactions or recreating the database, which keeps tests fast while ensuring isolation.

**No mocking in integration tests** — the full infrastructure stack runs against a real database. This catches issues that mocks would hide, like EF Core concurrency token behavior and outbox serialization.

**Unit tests test invariants, not workflows** — domain state transition rules are covered in unit tests. The integration tests cover the wiring between layers.
