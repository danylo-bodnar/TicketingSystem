using Ticketing.Domain.Hall.Exceptions;
using Ticketing.Domain.Halls;
using Ticketing.Domain.Seats;

namespace Ticketing.UnitTests.Halls
{
    public class HallTests
    {
        private Seat CreateSeat(Guid hallId, string row, int column)
            => new Seat(Guid.NewGuid(), hallId, row, column);

        [Fact]
        public void CreateHall_ShouldCreateWithSeats()
        {
            var hallId = Guid.NewGuid();
            var seats = new List<Seat>
            {
                CreateSeat(hallId, "A", 1),
                CreateSeat(hallId, "A", 2)
            };

            var hall = new Hall(hallId, "IMAX", seats);

            Assert.Equal("IMAX", hall.Name);
            Assert.Equal(2, hall.Seats.Count);
        }

        [Fact]
        public void CreateHallWithoutSeat_ShouldThrow_ArgumentException()
        {
            var hallId = Guid.NewGuid();
            var emptySeats = new List<Seat>();

            Assert.Throws<ArgumentException>(() =>
                new Hall(hallId, "Empty Hall", emptySeats)
            );
        }

        [Fact]
        public void AddSeat_ShouldAddSeatSuccessfully()
        {
            var hallId = Guid.NewGuid();
            var initialSeat = CreateSeat(hallId, "A", 1);
            var hall = new Hall(hallId, "Hall 1", new List<Seat> { initialSeat });

            var newSeat = CreateSeat(hallId, "A", 2);
            hall.AddSeat(newSeat);

            Assert.Equal(2, hall.Seats.Count);
            Assert.Contains(newSeat, hall.Seats);
        }

        [Fact]
        public void AddSeat_ShouldThrow_WhenDuplicateSeat()
        {
            var hallId = Guid.NewGuid();
            var seat1 = CreateSeat(hallId, "A", 1);
            var seat2 = CreateSeat(hallId, "A", 1);

            var hall = new Hall(hallId, "Hall 1", new List<Seat> { seat1 });

            Assert.Throws<SeatAlreadyExistsException>(() => hall.AddSeat(seat2));
        }

        [Fact]
        public void AddSeat_ShouldThrow_WhenSeatBelongsToAnotherHall()
        {
            var hall1Id = Guid.NewGuid();
            var hall2Id = Guid.NewGuid();

            var seatFromAnotherHall = CreateSeat(hall2Id, "A", 1);

            var hall = new Hall(hall1Id, "Hall 1", new List<Seat> { CreateSeat(hall1Id, "A", 1) });

            Assert.Throws<SeatBelongsToAnotherHallException>(() => hall.AddSeat(seatFromAnotherHall));
        }

        [Fact]
        public void GetSeat_ShouldReturnSeat()
        {
            var hallId = Guid.NewGuid();
            var seat = CreateSeat(hallId, "A", 1);

            var hall = new Hall(hallId, "Hall", new List<Seat> { seat });

            var result = hall.GetSeat(seat.Id);

            Assert.Equal(seat.Id, result.Id);
        }

        [Fact]
        public void GetSeat_ShouldThrow_WhenSeatNotFound()
        {
            var hallId = Guid.NewGuid();
            var hall = new Hall(hallId, "Hall", new List<Seat> { CreateSeat(hallId, "A", 1) });

            Assert.Throws<SeatNotFoundInHallException>(() => hall.GetSeat(Guid.NewGuid()));
        }
    }
}