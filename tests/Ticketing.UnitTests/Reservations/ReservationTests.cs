using Ticketing.Domain.Reservations;
using Ticketing.Domain.Seats.Exceptions;

namespace Ticketing.UnitTests.Reservations
{
    public class ReservationTests
    {
        [Fact]
        public void Confirm_WhenPending_ShouldSetStatusConfirmed()
        {
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

            reservation.Confirm();

            Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        }

        [Fact]
        public void Confirm_WhenNotPending_ShouldThrowException()
        {
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });
            reservation.Confirm();

            Assert.Throws<InvalidReservationStateException>(() => reservation.Confirm());
        }

        [Fact]
        public void Cancel_WhenPending_ShouldSetStatusCancelled()
        {
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

            reservation.Cancel();

            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        }

        [Fact]
        public void Expire_WhenPending_ShouldSetStatusExpired()
        {
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

            reservation.Expire();

            Assert.Equal(ReservationStatus.Expired, reservation.Status);
        }
    }
}