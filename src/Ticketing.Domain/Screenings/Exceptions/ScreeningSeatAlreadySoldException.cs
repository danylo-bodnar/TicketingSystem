using Ticketing.Domain.Common;

namespace Ticketing.Domain.Screenings.Exceptions
{
    public class ScreeningSeatAlreadySoldException : DomainException
    {
        public ScreeningSeatAlreadySoldException(Guid screeningSeatId)
            : base($"Screening seat {screeningSeatId} is already sold and cannot be released.")
        {
        }
    }
}