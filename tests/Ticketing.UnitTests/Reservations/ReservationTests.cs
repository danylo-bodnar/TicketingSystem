using Ticketing.Domain.Events;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Reservations.Exceptions;

namespace Ticketing.UnitTests.Reservations
{
    public class ReservationTests
    {
        private Reservation CreatePendingReservation()
        {
            return new Reservation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                [Guid.NewGuid()]
            );
        }

        [Fact]
        public void Create_ShouldSetPendingStatusAndDomainEvent()
        {
            var id = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var screeningId = Guid.NewGuid();
            var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var reservation = new Reservation(id, eventId, screeningId, seatIds);

            Assert.Equal(id, reservation.Id);
            Assert.Equal(eventId, reservation.EventId);
            Assert.Equal(screeningId, reservation.ScreeningId);
            Assert.Equal(2, reservation.SeatIds.Count);
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
            Assert.NotEqual(default, reservation.CreatedAt);
            Assert.NotNull(reservation.ExpiredAt);

            var domainEvent = Assert.Single(reservation.DomainEvents);
            var createdEvent = Assert.IsType<ReservationCreated>(domainEvent);
            Assert.Equal(id, createdEvent.ReservationId);
            Assert.Equal(screeningId, createdEvent.ScreeningId);
        }

        [Fact]
        public void Create_ShouldThrow_WhenNoSeats()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Reservation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), []));
            Assert.Contains("seat", ex.Message);
        }

        [Fact]
        public void Create_ShouldThrow_WhenSeatIdsNull()
        {
            Assert.Throws<ArgumentException>(() =>
                new Reservation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null!));
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
            reservation.Confirm();

            var ex = Assert.Throws<InvalidReservationStateException>(() => reservation.Confirm());
            Assert.Contains("Confirm", ex.Message);
        }

        [Fact]
        public void Confirm_ShouldRaiseDomainEvent()
        {
            var reservation = CreatePendingReservation();

            reservation.Confirm();

            var domainEvent = reservation.DomainEvents.Last();
            var confirmedEvent = Assert.IsType<ReservationConfirmed>(domainEvent);
            Assert.Equal(reservation.Id, confirmedEvent.ReservationId);
        }

        [Fact]
        public void Cancel_WhenPending_ShouldSetStatusCancelled()
        {
            var reservation = CreatePendingReservation();

            reservation.Cancel();

            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        }

        [Fact]
        public void Cancel_WhenNotPending_ShouldThrowException()
        {
            var reservation = CreatePendingReservation();
            reservation.Cancel();

            var ex = Assert.Throws<InvalidReservationStateException>(() => reservation.Cancel());
            Assert.Contains("Cancel", ex.Message);
        }

        [Fact]
        public void Cancel_ShouldNotRaiseDomainEvent()
        {
            var reservation = CreatePendingReservation();
            reservation.Cancel();
            Assert.Single(reservation.DomainEvents);
        }

        [Fact]
        public void Expire_WhenPending_ShouldSetStatusExpired()
        {
            var reservation = CreatePendingReservation();

            reservation.Expire();

            Assert.Equal(ReservationStatus.Expired, reservation.Status);
        }

        [Fact]
        public void Expire_WhenNotPending_ShouldThrowException()
        {
            var reservation = CreatePendingReservation();
            reservation.Expire();

            var ex = Assert.Throws<InvalidReservationStateException>(() => reservation.Expire());
            Assert.Contains("Expire", ex.Message);
        }
    }
}
