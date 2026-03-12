namespace Ticketing.Contracts.Reservations;

public record CreateReservationRequest(
    Guid ScreeningId,
    List<Guid> SeatIds
);