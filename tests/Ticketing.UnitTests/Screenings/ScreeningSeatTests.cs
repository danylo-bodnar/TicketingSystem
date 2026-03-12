using Ticketing.Domain.Screenings;
using Ticketing.Domain.Screenings.Exceptions;

namespace Ticketing.UnitTests.Screenings
{
    public class ScreeningSeatTests
    {
        [Fact]
        public void Reserve_ShouldSetStatusToReserved()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            seat.Reserve();

            Assert.Equal(ScreeningSeatStatus.Reserved, seat.Status);
        }

        [Fact]
        public void Reserve_ShouldThrow_WhenAlreadyReservedOrSold()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            seat.Reserve();

            var ex = Assert.Throws<ScreeningSeatNotAvailableException>(() => seat.Reserve());
            Assert.Contains("is not available", ex.Message);
        }

        [Fact]
        public void MarkAsSold_ShouldSetStatusToSold()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            seat.Reserve();

            seat.MarkAsSold();

            Assert.Equal(ScreeningSeatStatus.Sold, seat.Status);
        }

        [Fact]
        public void MarkAsSold_ShouldThrow_WhenSeatNotReserved()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var ex = Assert.Throws<ScreeningSeatNotReservedException>(() => seat.MarkAsSold());
            Assert.Contains("must be reserved first", ex.Message);
        }

        [Fact]
        public void Release_ShouldSetStatusToAvailable()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            seat.Reserve();

            seat.Release();

            Assert.Equal(ScreeningSeatStatus.Available, seat.Status);
        }

        [Fact]
        public void Release_ShouldThrow_WhenSeatAlreadySold()
        {
            var seat = new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            seat.Reserve();
            seat.MarkAsSold();

            var ex = Assert.Throws<ScreeningSeatAlreadySoldException>(() => seat.Release());
            Assert.Contains("already sold", ex.Message);
        }
    }
}