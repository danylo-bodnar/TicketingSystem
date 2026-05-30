using Microsoft.Extensions.Logging;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Screenings.Exceptions;

namespace Ticketing.UnitTests.Application;

public class ReservationCreatedEventHandlerTests
{
    private readonly Mock<IPaymentRepository> _paymentMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IScreeningRepository> _screeningMock;
    private readonly ReservationCreatedHandler _sut;

    public ReservationCreatedEventHandlerTests()
    {
        _paymentMock = new Mock<IPaymentRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _screeningMock = new Mock<IScreeningRepository>();
        var loggerMock = new Mock<ILogger<ReservationCreatedHandler>>();
        _sut = new ReservationCreatedHandler(_paymentMock.Object, _uowMock.Object, _screeningMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreatePayment()
    {
        var screeningId = Guid.NewGuid();
        var screening = new Screening(screeningId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid())]);
        _screeningMock.Setup(r => r.GetByIdAsync(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync(screening);

        var @event = new ReservationCreated
        {
            ReservationId = Guid.NewGuid(),
            ScreeningId = screeningId,
            SeatIds = [Guid.NewGuid()],
            CreatedAt = DateTime.UtcNow
        };

        await _sut.HandleAsync(@event, CancellationToken.None);

        _paymentMock.Verify(r => r.AddAsync(It.Is<Payment>(p => p.ReservationId == @event.ReservationId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenScreeningNotFound_ShouldThrow()
    {
        var screeningId = Guid.NewGuid();
        _screeningMock.Setup(r => r.GetByIdAsync(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync((Screening?)null);

        var @event = new ReservationCreated
        {
            ReservationId = Guid.NewGuid(),
            ScreeningId = screeningId,
            SeatIds = [Guid.NewGuid()],
            CreatedAt = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<ScreeningNotFoundException>(() =>
            _sut.HandleAsync(@event, CancellationToken.None));
    }
}
