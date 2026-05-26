using Ticketing.Domain.Reservations;
using Ticketing.Domain.Reservations.Exceptions;

namespace Ticketing.UnitTests.Reservations
{
    public class ReservationTests
    {
        private Reservation CreatePendingReservation()
        {
            return new Reservation(
                Guid.NewGuid(),                 // reservationId
                Guid.NewGuid(),                 // eventId
                Guid.NewGuid(),                 // screeningId
                new List<Guid> { Guid.NewGuid() } // seatIds
            );
        }

        [Fact]
        public void Confirm_WhenPending_ShouldSetStatusConfirmed()
        {
            var reservation = CreatePendingReservation();

            reservation.Confirm();

            Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        }

        [Fact]
        public void Confirm_WhenNotPending_ShouldThrowException()
        {
            var reservation = CreatePendingReservation();
            reservation.Confirm(); // move to Confirmed first

            Assert.Throws<InvalidReservationStateException>(() => reservation.Confirm());
        }

        [Fact]
        public void Cancel_WhenPending_ShouldSetStatusCancelled()
        {
            var reservation = CreatePendingReservation();

            reservation.Cancel();

            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        }

        [Fact]
        public void Expire_WhenPending_ShouldSetStatusExpired()
        {
            var reservation = CreatePendingReservation();

            reservation.Expire();

            Assert.Equal(ReservationStatus.Expired, reservation.Status);
        }
    }
}
