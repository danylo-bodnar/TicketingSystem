namespace Ticketing.Domain.Events
{
    public sealed class ReservationConfirmed : IEvent
    {
        public Guid ReservationId { get; }
        public List<Guid> SeatIds { get; private set; } = new();
        public Guid ScreeningId { get; }
        public DateTime ConfirmedAt { get; }

        public ReservationConfirmed(Guid reservationId, Guid screeningId, List<Guid> seatIds, DateTime confirmedAt)
        {
            ReservationId = reservationId;
            ScreeningId = screeningId;
            SeatIds = seatIds;
            ConfirmedAt = confirmedAt;
        }
    }
}
