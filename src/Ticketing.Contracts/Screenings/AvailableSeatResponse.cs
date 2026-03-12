namespace Ticketing.Contracts.Screenings;

public record AvailableSeatResponse(
    Guid SeatId,
    string Row,
    int Number
);