namespace Ticketing.Domain.Events
{
    public sealed class ReservationConfirmed : IEvent
    {
        public Guid ReservationId { get; }
        public Guid ScreeningId { get; }
        public DateTime ConfirmedAt { get; }

        public ReservationConfirmed(Guid reservationId, Guid screeningId, DateTime confirmedAt)
        {
            ReservationId = reservationId;
            ScreeningId = screeningId;
            ConfirmedAt = confirmedAt;
        }
    }
}
