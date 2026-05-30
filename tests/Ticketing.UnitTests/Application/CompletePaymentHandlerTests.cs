using Microsoft.Extensions.Logging;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Payments.Commands;
using Ticketing.Application.Payments.Handlers;
using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Payments;

namespace Ticketing.UnitTests.Application;

public class CompletePaymentHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CompletePaymentHandler _sut;

    public CompletePaymentHandlerTests()
    {
        _paymentMock = new Mock<IPaymentRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<CompletePaymentHandler>>();
        _sut = new CompletePaymentHandler(_paymentMock.Object, _uowMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCompletePayment()
    {
        var payment = new Payment(Guid.NewGuid(), new Money(100, "USD"));
        _paymentMock.Setup(r => r.GetById(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        await _sut.Handle(new CompletePaymentCommand(payment.Id), CancellationToken.None);

        Assert.Equal(PaymentStatus.Completed, payment.Status);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPaymentNotFound_ShouldThrow()
    {
        var paymentId = Guid.NewGuid();
        _paymentMock.Setup(r => r.GetById(paymentId, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(new CompletePaymentCommand(paymentId), CancellationToken.None));
    }
}
