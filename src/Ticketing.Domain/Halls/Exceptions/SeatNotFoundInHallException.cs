using Ticketing.Domain.Common;

namespace Ticketing.Domain.Hall.Exceptions
{
    public class SeatNotFoundInHallException : DomainException
    {
        public SeatNotFoundInHallException(Guid hallId, Guid seatId)
            : base($"Seat {seatId} not found in hall {hallId}")
        {
        }
    }
}