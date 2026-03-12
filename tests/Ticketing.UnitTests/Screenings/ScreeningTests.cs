using Ticketing.Domain.Screenings;

namespace Ticketing.UnitTests.Screenings
{
    public class ScreeningTests
    {
        private ScreeningSeat CreateSeat(Guid screeningId)
            => new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());

        [Fact]
        public void CreateScreening_ShouldRequireSeats()
        {
            var screeningId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() =>
                new Screening(screeningId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, new List<ScreeningSeat>())
            );
        }

        [Fact]
        public void GetSeat_ShouldReturnCorrectSeat()
        {
            var screeningId = Guid.NewGuid();
            var seat = CreateSeat(screeningId);
            var screening = new Screening(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, new List<ScreeningSeat> { seat });

            var result = screening.GetSeat(seat.SeatId);

            Assert.Equal(seat.SeatId, result.SeatId);
        }

        [Fact]
        public void GetSeat_ShouldThrow_WhenSeatNotFound()
        {
            var screeningId = Guid.NewGuid();
            var existingSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
            var screening = new Screening(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, new List<ScreeningSeat> { existingSeat });

            Assert.Throws<Exception>(() => screening.GetSeat(Guid.NewGuid()));
        }
    }
}