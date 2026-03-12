namespace Ticketing.Contracts.Screenings;

public record ScreeningResponse
(
    Guid Id,
    Guid EventId,
    DateTime StartTime
);
