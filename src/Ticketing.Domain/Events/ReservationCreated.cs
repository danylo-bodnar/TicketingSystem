namespace Ticketing.Domain.Events
{
    public sealed class ReservationCreated : IEvent
    {
        public Guid ReservationId { get; }
        public Guid ScreeningId { get; }
        public List<Guid> SeatIds { get; }
        public DateTime CreatedAt { get; }

        public ReservationCreated(
            Guid reservationId,
            Guid screeningId,
            List<Guid> seatIds,
            DateTime createdAt)
        {
            ReservationId = reservationId;
            ScreeningId = screeningId;
            SeatIds = seatIds;
            CreatedAt = createdAt;
        }
    }
}
