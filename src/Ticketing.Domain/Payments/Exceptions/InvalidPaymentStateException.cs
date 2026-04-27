using Ticketing.Domain.Common;

namespace Ticketing.Domain.Payments.Exceptions
{
    public class InvalidPaymentStateException : DomainException
    {
        public InvalidPaymentStateException(Guid paymentId, PaymentStatus status, string attemptedAction) : base($"Cannot perform '{attemptedAction}' on payment '{paymentId}' due to invalid state {status}.")
        {
        }
    }
}