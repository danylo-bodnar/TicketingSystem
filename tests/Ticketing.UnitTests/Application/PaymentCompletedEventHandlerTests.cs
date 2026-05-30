using Microsoft.Extensions.Logging;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.EventHandlers;
using Ticketing.Domain.Events;
using Ticketing.Domain.Reservations;

namespace Ticketing.UnitTests.Application;

public class PaymentCompletedEventHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly PaymentCompletedHandler _sut;

    public PaymentCompletedEventHandlerTests()
    {
        _reservationMock = new Mock<IReservationRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<PaymentCompletedHandler>>();
        _sut = new PaymentCompletedHandler(_reservationMock.Object, _uowMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldConfirmReservation()
    {
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation(reservationId, Guid.NewGuid(), Guid.NewGuid(), [Guid.NewGuid()]);
        _reservationMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync(reservation);

        await _sut.HandleAsync(new PaymentCompleted
        {
            PaymentId = Guid.NewGuid(),
            ReservationId = reservationId,
            Amount = 100,
            Currency = "USD"
        }, CancellationToken.None);

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenReservationNotFound_ShouldThrow()
    {
        var reservationId = Guid.NewGuid();
        _reservationMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.HandleAsync(new PaymentCompleted
            {
                PaymentId = Guid.NewGuid(),
                ReservationId = reservationId,
                Amount = 100,
                Currency = "USD"
            }, CancellationToken.None));
    }
}
