using Ticketing.Domain.Common;

namespace Ticketing.Domain.Screenings.Exceptions
{
    public class ScreeningSeatNotReservedException : DomainException
    {
        public ScreeningSeatNotReservedException(Guid screeningSeatId)
          : base($"Screening seat {screeningSeatId} must be reserved first to mark as sold.")
        {
        }
    }
}