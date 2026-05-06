namespace Ticketing.Domain.Events
{
    public record ReservationConfirmed : IEvent
    {
        public Guid ReservationId { get; init; }
        public Guid ScreeningId { get; init; }
        public List<Guid> SeatIds { get; init; } = new();
        public DateTime ConfirmedAt { get; init; }
    }
}
