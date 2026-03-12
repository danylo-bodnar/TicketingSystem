using Ticketing.Domain.Seats;

namespace Ticketing.UnitTests.Seats
{
    public class SeatTests
    {
        [Fact]
        public void CreateSeat_ShouldAssignProperties()
        {
            var id = Guid.NewGuid();
            var hallId = Guid.NewGuid();
            var seat = new Seat(id, hallId, "A", 5);

            Assert.Equal(id, seat.Id);
            Assert.Equal(hallId, seat.HallId);
            Assert.Equal("A", seat.Row);
            Assert.Equal(5, seat.Column);
        }

        [Fact]
        public void CreateSeat_ShouldThrow_WhenRowIsEmpty()
        {
            var id = Guid.NewGuid();
            var hallId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => new Seat(id, hallId, "", 1));
            Assert.Throws<ArgumentException>(() => new Seat(id, hallId, " ", 1));
            Assert.Throws<ArgumentException>(() => new Seat(id, hallId, null!, 1));
        }

        [Fact]
        public void CreateSeat_ShouldThrow_WhenColumnIsZeroOrNegative()
        {
            var id = Guid.NewGuid();
            var hallId = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => new Seat(id, hallId, "A", 0));
            Assert.Throws<ArgumentException>(() => new Seat(id, hallId, "A", -1));
        }
    }
}