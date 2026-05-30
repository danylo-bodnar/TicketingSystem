using Ticketing.Domain.Screenings;

namespace Ticketing.UnitTests.Screenings
{
    public class ScreeningTests
    {
        private readonly ScreeningSeat _availableSeat;
        private readonly ScreeningSeat _reservedSeat;
        private readonly ScreeningSeat _soldSeat;
        private readonly Screening _screening;

        public ScreeningTests()
        {
            var screeningId = Guid.NewGuid();
            _availableSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
            _reservedSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
            _reservedSeat.Reserve();
            _soldSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
            _soldSeat.Reserve();
            _soldSeat.MarkAsSold();

            _screening = new Screening(
                screeningId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow,
                new List<ScreeningSeat> { _availableSeat, _reservedSeat, _soldSeat });
        }

        [Fact]
        public void Create_ShouldSetProperties()
        {
            var id = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var hallId = Guid.NewGuid();
            var start = DateTime.UtcNow;
            var seat = new ScreeningSeat(Guid.NewGuid(), id, Guid.NewGuid());

            var screening = new Screening(id, eventId, hallId, start, new List<ScreeningSeat> { seat });

            Assert.Equal(id, screening.Id);
            Assert.Equal(eventId, screening.EventId);
            Assert.Equal(hallId, screening.HallId);
            Assert.Equal(start, screening.StartTime);
            Assert.Single(screening.Seats);
        }

        [Fact]
        public void Create_ShouldThrow_WhenNoSeats()
        {
            Assert.Throws<ArgumentException>(() =>
                new Screening(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, []));
        }

        [Fact]
        public void GetSeat_ShouldReturnCorrectSeat()
        {
            var result = _screening.GetSeat(_availableSeat.SeatId);

            Assert.Equal(_availableSeat.SeatId, result.SeatId);
        }

        [Fact]
        public void GetSeat_ShouldThrow_WhenSeatNotFound()
        {
            Assert.Throws<ScreeningSeatNotFoundException>(() => _screening.GetSeat(Guid.NewGuid()));
        }

        [Fact]
        public void GetAvailableSeats_ShouldReturnOnlyAvailable()
        {
            var available = _screening.GetAvailableSeats().ToList();

            Assert.Single(available);
            Assert.Equal(_availableSeat.SeatId, available[0].SeatId);
        }

        [Fact]
        public void GetAvailableSeatCount_ShouldReturnCount()
        {
            Assert.Equal(1, _screening.GetAvailableSeatCount());
        }

        [Fact]
        public void GetTotalSeatCount_ShouldReturnTotal()
        {
            Assert.Equal(3, _screening.GetTotalSeatCount());
        }

        [Fact]
        public void GetOccupancyRatio_WhenFullyAvailable_ShouldReturnZero()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var screening = new Screening(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [seat]);

            Assert.Equal(0, screening.GetOccupancyRatio());
        }

        [Fact]
        public void GetOccupancyRatio_WhenMixed_ShouldReturnCorrectRatio()
        {
            Assert.Equal(2m / 3m, _screening.GetOccupancyRatio());
        }

        [Fact]
        public void GetOccupancyRatio_WhenAllOccupied_ShouldReturnOne()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            seat.Reserve();
            seat.MarkAsSold();
            var screening = new Screening(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [seat]);

            Assert.Equal(1, screening.GetOccupancyRatio());
        }
    }
}
