using Ticketing.Domain.Hall.Exceptions;
using Ticketing.Domain.Seats;

namespace Ticketing.Domain.Halls
{
    public class Hall
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public List<Seat> Seats { get; private set; } = new();

        private Hall() { }

        public Hall(Guid id, string name, List<Seat> seats)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Hall name is required");
            }

            if (seats == null || seats.Count == 0)
            {
                throw new ArgumentException("Hall must have at least one seat");
            }

            Id = id;
            Name = name;
            Seats = seats;
        }

        public Seat GetSeat(Guid seatId)
        {
            var seat = Seats.FirstOrDefault(s => s.Id == seatId);

            if (seat == null)
            {
                throw new SeatNotFoundInHallException(Id, seatId);
            }

            return seat;
        }

        public void AddSeat(Seat seat)
        {
            if (seat.HallId != Id)
            {
                throw new SeatBelongsToAnotherHallException(Id, seat.HallId);
            }

            if (Seats.Any(s => s.Row == seat.Row && s.Column == seat.Column))
            {
                throw new SeatAlreadyExistsException(seat.Row, seat.Column);
            }

            Seats.Add(seat);
        }
    }
}