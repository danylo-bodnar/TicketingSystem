using Ticketing.Domain.Common;

namespace Ticketing.Domain.Screenings.Exceptions
{
    public class ScreeningSeatNotAvailableException : DomainException
    {
        public ScreeningSeatNotAvailableException(Guid screeningSeatId)
            : base($"Screening seat {screeningSeatId} is not available for reservation.")
        {
        }
    }
}