using Ticketing.Domain.Common;

namespace Ticketing.Domain.Hall.Exceptions
{
    public class SeatAlreadyExistsException : DomainException
    {
        public SeatAlreadyExistsException(string row, int column)
          : base($"Seat at row {row}, column {column} already exists")
        {
        }
    }
}