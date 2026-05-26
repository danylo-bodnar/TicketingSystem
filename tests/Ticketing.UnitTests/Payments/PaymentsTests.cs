using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Payments.Exceptions;

namespace Ticketing.UnitTests.Payments
{
    public class PaymentsTests
    {
        [Fact]
        public void Complete_WhenPending_ShouldSetStatusCompleted()
        {
            var payment = new Payment(Guid.NewGuid(), new Money(100, "USD"));

            payment.Complete();

            Assert.Equal(PaymentStatus.Completed, payment.Status);
        }

        [Fact]
        public void Complete_WhenNotPending_ShouldThrowException()
        {
            var payment = new Payment(Guid.NewGuid(), new Money(100, "USD"));
            payment.Complete();

            Assert.Throws<InvalidPaymentStateException>(() => payment.Complete());
        }
    }
}