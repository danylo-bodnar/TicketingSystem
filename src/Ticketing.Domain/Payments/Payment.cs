using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Payments.Exceptions;

namespace Ticketing.Domain.Payments
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid ReservationId { get; private set; }
        public Money Amount { get; private set; } = null!;
        public PaymentStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Payment() { }

        public Payment(Guid id, Guid reservationId, Money amount)
        {
            Id = id;
            ReservationId = reservationId;
            Amount = amount;
            Status = PaymentStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != PaymentStatus.Pending)
            {
                throw new InvalidPaymentStateException(Id, nameof(Complete));
            }

            Status = PaymentStatus.Completed;
        }

        public void Fail()
        {
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