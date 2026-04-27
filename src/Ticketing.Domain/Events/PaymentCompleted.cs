namespace Ticketing.Domain.Events
{
    public record PaymentCompleted(
        Guid PaymentId,
        Guid ReservationId,
        decimal Amount,
        string Currency
    ) : IEvent;
}