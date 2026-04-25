using Ticketing.Domain.Common;

namespace Ticketing.Domain.Halls.Exceptions
{
    public class SeatNotFoundException : DomainException
    {
        public SeatNotFoundException(Guid seatId)
            : base($"Seat {seatId} not found ")
        {
        }
    }
}
