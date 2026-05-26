using Ticketing.Domain.Common;

namespace Ticketing.Domain.Halls.Exceptions
{
    public class SeatBelongsToAnotherHallException : DomainException
    {
        public SeatBelongsToAnotherHallException(Guid hallId, Guid seatHallId)
                   : base($"Seat belongs to hall {seatHallId} but was added to hall {hallId}")
        {
        }
    }
}