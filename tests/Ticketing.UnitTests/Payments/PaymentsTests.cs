using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Payments.Exceptions;

namespace Ticketing.UnitTests.Payments
{
    public class PaymentsTests
    {
        private Payment CreatePendingPayment() => new(Guid.NewGuid(), new Money(100, "USD"));

        [Fact]
        public void Complete_WhenPending_ShouldSetStatusCompleted()
        {
            var payment = CreatePendingPayment();

            payment.Complete();

            Assert.Equal(PaymentStatus.Completed, payment.Status);
        }

        [Fact]
        public void Complete_WhenNotPending_ShouldThrowException()
        {
            var payment = CreatePendingPayment();
            payment.Complete();

            var ex = Assert.Throws<InvalidPaymentStateException>(() => payment.Complete());
            Assert.Contains("Complete", ex.Message);
        }

        [Fact]
        public void Fail_WhenPending_ShouldSetStatusFailed()
        {
            var payment = CreatePendingPayment();

            payment.Fail();

            Assert.Equal(PaymentStatus.Failed, payment.Status);
        }

        [Fact]
        public void Fail_WhenNotPending_ShouldThrowException()
        {
            var payment = CreatePendingPayment();
            payment.Fail();

            var ex = Assert.Throws<InvalidPaymentStateException>(() => payment.Fail());
            Assert.Contains("Fail", ex.Message);
        }

        [Fact]
        public void Complete_ShouldRaiseDomainEvent()
        {
            var payment = CreatePendingPayment();

            payment.Complete();

            var domainEvent = payment.DomainEvents.Single();
            Assert.IsType<PaymentCompleted>(domainEvent);
        }

        [Fact]
        public void Fail_ShouldNotRaiseDomainEvent()
        {
            var payment = CreatePendingPayment();

            payment.Fail();

            Assert.Empty(payment.DomainEvents);
        }
    }
}