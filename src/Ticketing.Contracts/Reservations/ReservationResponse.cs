namespace Ticketing.Contracts.Reservations;

public record ReservationResponse(
    Guid ReservationId,
    Guid ScreeningId,
    List<Guid> SeatIds,
    string Status
);