using Ticketing.Domain.Common;

namespace Ticketing.Domain.Reservations.Exceptions
{
    public class InvalidReservationStateException : DomainException
    {
        public InvalidReservationStateException(Guid reservationId, string attemptedAction)
            : base($"Cannot perform '{attemptedAction}' on reservation '{reservationId}' due to invalid state.")
        {
        }
    }
}