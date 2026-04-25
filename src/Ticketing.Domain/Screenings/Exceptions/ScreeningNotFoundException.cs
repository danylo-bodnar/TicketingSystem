using Ticketing.Domain.Common;

namespace Ticketing.Domain.Screenings.Exceptions
{
    public class ScreeningNotFoundException : DomainException
    {
        public ScreeningNotFoundException(Guid screeningSeatId)
            : base($"Screening {screeningSeatId} is not found.")
        {
        }
    }
}
