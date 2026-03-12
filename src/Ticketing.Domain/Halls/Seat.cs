namespace Ticketing.Domain.Seats
{
    public class Seat
    {
        public Guid Id { get; private set; }
        public Guid HallId { get; private set; }

        public string Row { get; private set; } = null!;
        public int Column { get; private set; }

        private Seat() { }

        public Seat(Guid id, Guid hallId, string row, int column)
        {
            if (string.IsNullOrWhiteSpace(row))
            {
                throw new ArgumentException("Row cannot be empty");
            }

            if (column <= 0)
            {
                throw new ArgumentException("Column must be greater than 0");
            }

            Id = id;
            HallId = hallId;
            Row = row;
            Column = column;
        }
    }
}