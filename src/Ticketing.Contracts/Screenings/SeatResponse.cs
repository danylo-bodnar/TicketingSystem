namespace Ticketing.Contracts.Seats;

public record SeatResponse(
    Guid Id,
    string Row,
    int Number
);