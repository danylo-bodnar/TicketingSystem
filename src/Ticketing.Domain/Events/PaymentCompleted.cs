namespace Ticketing.Domain.Events
{
    public record PaymentCompleted : IEvent
    {
        public Guid PaymentId { get; init; }
        public Guid ReservationId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
    }
}