namespace Ticketing.Domain.Screenings
{
    public class Screening
    {
        public Guid Id { get; private set; }

        public Guid EventId { get; private set; }

        public Guid HallId { get; private set; }

        public DateTime StartTime { get; private set; }

        public List<ScreeningSeat> Seats { get; private set; } = new();

        private Screening() { }

        public Screening(Guid id, Guid eventId, Guid hallId, DateTime startTime, List<ScreeningSeat> seats)
        {
            if (seats == null || seats.Count == 0)
                throw new ArgumentException("Screening must have seats");

            Id = id;
            EventId = eventId;
            HallId = hallId;
            StartTime = startTime;
            Seats = seats;
        }

        public ScreeningSeat GetSeat(Guid seatId)
        {
            var seat = Seats.FirstOrDefault(x => x.SeatId == seatId);

            if (seat == null)
                throw new Exception("Seat not found in screening");

            return seat;
        }

        public IEnumerable<ScreeningSeat> GetAvailableSeats()
        {
            return Seats.Where(x => x.Status == ScreeningSeatStatus.Available);
        }
    }
}