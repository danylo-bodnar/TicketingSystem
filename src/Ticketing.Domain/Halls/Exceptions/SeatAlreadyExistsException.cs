using Ticketing.Domain.Common;

namespace Ticketing.Domain.Halls.Exceptions
{
    public class SeatAlreadyExistsException : DomainException
    {
        public SeatAlreadyExistsException(string row, int column)
          : base($"Seat at row {row}, column {column} already exists")
        {
        }
    }
}