using Ticketing.Domain.Events;
using Ticketing.Domain.Seats.Exceptions;

namespace Ticketing.Domain.Reservations
{
    public class Reservation : AggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid ScreeningId { get; private set; }
        public List<Guid> SeatIds { get; private set; } = new();
        public ReservationStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ExpiredAt { get; private set; }

        private Reservation() { }

        public Reservation(Guid id, Guid eventId, Guid screeningId, List<Guid> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0)
            {
                throw new ArgumentException("Must reserve at least one seat");
            }

            Id = id;
            EventId = eventId;
            ScreeningId = screeningId;
            SeatIds = seatIds;
            Status = ReservationStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            ExpiredAt = CreatedAt.AddMinutes(5);

            AddDomainEvent(new ReservationCreated(
                        id,
                        screeningId,
                        seatIds,
                        DateTime.UtcNow
                    ));
        }

        public void Confirm()
        {
            if (Status != ReservationStatus.Pending)
            {
                throw new InvalidReservationStateException(Id, nameof(Confirm));
            }

            Status = ReservationStatus.Confirmed;

            AddDomainEvent(new ReservationConfirmed(
                   Id,
                   ScreeningId,
                   DateTime.UtcNow
               ));
        }

        public void Cancel()
        {
            if (Status == ReservationStatus.Confirmed)
            {
                throw new InvalidReservationStateException(Id, nameof(Cancel));
            }

            Status = ReservationStatus.Cancelled;
        }

        public void Expire()
        {
            if (Status == ReservationStatus.Confirmed)
            {
                throw new InvalidReservationStateException(Id, nameof(Expire));
            }

            Status = ReservationStatus.Expired;
        }
    }

    public enum ReservationStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Expired
    }
}