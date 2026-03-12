using Ticketing.Domain.Common;

namespace Ticketing.Domain.Payments.Exceptions
{
    public class PaymentAlreadyProcessedException : DomainException
    {
        public PaymentAlreadyProcessedException(Guid paymentId) : base($"Payment '{paymentId}' has already been processed.")
        {
        }
    }
}