# Domain Model

## Overview

The domain is organized around five core concepts: halls, seats, screenings, reservations, and payments. Two of these — `Reservation` and `Payment` — are aggregates that own domain logic and raise domain events. The rest are entities or value objects that support them.

```
Hall
 └── Seat[]

Screening
 └── ScreeningSeat[]  (projection of Seat into a specific screening)

Reservation  ← AggregateRoot
Payment      ← AggregateRoot
```

## Aggregates

### Reservation

Represents a user's intent to purchase seats for a screening. Created with a 5-minute expiry window.

**State transitions:**

```
Pending → Confirmed
Pending → Cancelled
Pending → Expired
```

A confirmed reservation cannot be cancelled or expired.

**Domain events raised:**

| Event                  | When            |
| ---------------------- | --------------- |
| `ReservationCreated`   | On construction |
| `ReservationConfirmed` | On `Confirm()`  |

**Key properties:**

| Property      | Type                | Notes                          |
| ------------- | ------------------- | ------------------------------ |
| `Id`          | `Guid`              |                                |
| `EventId`     | `Guid`              |                                |
| `ScreeningId` | `Guid`              |                                |
| `SeatIds`     | `List<Guid>`        | Physical seat references       |
| `Status`      | `ReservationStatus` |                                |
| `CreatedAt`   | `DateTime`          |                                |
| `ExpiredAt`   | `DateTime?`         | Set to `CreatedAt + 5 minutes` |

```csharp
public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Expired
}
```

---

### Payment

Represents the financial transaction tied to a reservation.

**State transitions:**

```
Pending → Completed
Pending → Failed
```

A payment that is already completed or failed cannot transition again.

**Domain events raised:**

| Event              | When            |
| ------------------ | --------------- |
| `PaymentCompleted` | On `Complete()` |

**Key properties:**

| Property        | Type            | Notes        |
| --------------- | --------------- | ------------ |
| `Id`            | `Guid`          |              |
| `ReservationId` | `Guid`          |              |
| `Amount`        | `Money`         | Value object |
| `Status`        | `PaymentStatus` |              |
| `CreatedAt`     | `DateTime`      |              |

```csharp
public enum PaymentStatus
{
    Pending,
    Completed,
    Failed
}
```

## Entities

### Hall

Represents a physical venue. Owns a collection of `Seat` entities.

**Invariants enforced:**

- Name cannot be empty
- Must have at least one seat
- No two seats can share the same row and column
- A seat can only be added if its `HallId` matches the hall

### Seat

Represents a physical seat in a hall. Immutable after creation.

| Property | Type     |
| -------- | -------- |
| `Id`     | `Guid`   |
| `HallId` | `Guid`   |
| `Row`    | `string` |
| `Column` | `int`    |

Row cannot be empty, column must be greater than 0.

### Screening

Represents a specific showing of an event in a hall at a given time. Owns a collection of `ScreeningSeat` entities.

**Useful queries exposed:**

- `GetAvailableSeats()` — seats with status `Available`
- `GetAvailableSeatCount()` / `GetTotalSeatCount()`
- `GetOccupancyRatio()` — fraction of seats that are reserved or sold

### ScreeningSeat

A projection of a physical `Seat` into a specific `Screening`. Tracks booking status and is the primary concurrency target.

**State transitions:**

```
Available → Reserved → Sold
Reserved  → Available  (via Release)
```

A sold seat cannot be released.

**Concurrency token:**

```csharp
[Timestamp]
public uint Version { get; private set; }
```

EF Core uses `Version` to detect concurrent updates. See [concurrency.md](./concurrency.md) for details.

```csharp
public enum ScreeningSeatStatus
{
    Available = 0,
    Reserved  = 1,
    Sold      = 2
}
```

## Value Objects

### Money

Wraps `Amount` (decimal) and `Currency` (string). Used on `Payment` to avoid primitive obsession on financial values.

## Domain Events

Events are raised inside aggregates via `AddDomainEvent()` and persisted to the outbox in the same transaction as the state change. See [outbox-pattern.md](./outbox-pattern.md) for how they are dispatched.

| Event                  | Raised by                 | Triggers                 |
| ---------------------- | ------------------------- | ------------------------ |
| `ReservationCreated`   | `Reservation` constructor | Payment creation         |
| `ReservationConfirmed` | `Reservation.Confirm()`   | Seat marking as sold     |
| `PaymentCompleted`     | `Payment.Complete()`      | Reservation confirmation |

## Invariants Summary

| Rule                                                 | Enforced in                   |
| ---------------------------------------------------- | ----------------------------- |
| Seat can only be reserved if `Available`             | `ScreeningSeat.Reserve()`     |
| Seat can only be sold if `Reserved`                  | `ScreeningSeat.MarkAsSold()`  |
| Sold seat cannot be released                         | `ScreeningSeat.Release()`     |
| Reservation can only be confirmed if `Pending`       | `Reservation.Confirm()`       |
| Confirmed reservation cannot be cancelled or expired | `Reservation.Cancel/Expire()` |
| Payment can only complete or fail if `Pending`       | `Payment.Complete/Fail()`     |
| Hall seat positions must be unique                   | `Hall.AddSeat()`              |
