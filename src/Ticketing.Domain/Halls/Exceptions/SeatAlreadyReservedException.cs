using Ticketing.Domain.Common;

namespace Ticketing.Domain.Seats.Exceptions
{
    public class SeatAlreadyReservedException : DomainException
    {
        public SeatAlreadyReservedException(Guid seatId) : base($"Seat '{seatId}' is already reserved.")
        {
        }
    }
}