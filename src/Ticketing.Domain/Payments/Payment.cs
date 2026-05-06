using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments.Exceptions;

namespace Ticketing.Domain.Payments
{
    public class Payment : AggregateRoot
    {
        public Guid Id { get; private set; }
        public Guid ReservationId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Payment() { }

        public Payment(Guid reservationId, Money amount)
        {
            Id = Guid.NewGuid();
            ReservationId = reservationId;
            Amount = amount;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidPaymentStateException(Id, Status, nameof(Complete));
            }

            Status = PaymentStatus.Completed;
            AddDomainEvent(new PaymentCompleted
            {
                PaymentId = Id,
                ReservationId = ReservationId,
                Amount = Amount.Amount,
                Currency = Amount.Currency
            });
        }

        public void Fail()
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidPaymentStateException(Id, Status, nameof(Fail));
            }
            Status = PaymentStatus.Failed;
        }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed
    }
}