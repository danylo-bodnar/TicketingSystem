using Ticketing.Domain.Common;

public class ScreeningSeatNotFoundException : DomainException
{
    public ScreeningSeatNotFoundException(Guid seatId)
        : base($"Seat {seatId} was not found in this screening")
    {
    }
}
