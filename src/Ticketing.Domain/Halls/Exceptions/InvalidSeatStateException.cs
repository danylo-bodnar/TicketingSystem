using Ticketing.Domain.Common;

namespace Ticketing.Domain.Seats.Exceptions
{
    public class InvalidSeatStateException : DomainException
    {
        public InvalidSeatStateException(Guid seatId, string attemptedAction) : base($"Cannot perform '{attemptedAction}' on seat '{seatId}' due to invalid state.")
        {
        }
    }
}