using Microsoft.Extensions.Logging;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.EventHandlers;
using Ticketing.Domain.Events;
using Ticketing.Domain.Screenings;

namespace Ticketing.UnitTests.Application;

public class ReservationConfirmedEventHandlerTests
{
    private readonly Mock<IScreeningRepository> _screeningMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ReservationConfirmedHandler _sut;

    public ReservationConfirmedEventHandlerTests()
    {
        _screeningMock = new Mock<IScreeningRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        var loggerMock = new Mock<ILogger<ReservationConfirmedHandler>>();
        _sut = new ReservationConfirmedHandler(_screeningMock.Object, _uowMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldMarkSeatsAsSold()
    {
        var screeningId = Guid.NewGuid();
        var seat1 = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
        seat1.Reserve();
        var seat2 = new ScreeningSeat(Guid.NewGuid(), screeningId, Guid.NewGuid());
        seat2.Reserve();
        var screening = new Screening(screeningId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [seat1, seat2]);
        _screeningMock.Setup(r => r.GetByIdAsync(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync(screening);

        await _sut.HandleAsync(new ReservationConfirmed
        {
            ReservationId = Guid.NewGuid(),
            ScreeningId = screeningId,
            SeatIds = [seat1.SeatId, seat2.SeatId],
            ConfirmedAt = DateTime.UtcNow
        }, CancellationToken.None);

        Assert.Equal(ScreeningSeatStatus.Sold, seat1.Status);
        Assert.Equal(ScreeningSeatStatus.Sold, seat2.Status);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenScreeningNotFound_ShouldThrow()
    {
        var screeningId = Guid.NewGuid();
        _screeningMock.Setup(r => r.GetByIdAsync(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync((Screening?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.HandleAsync(new ReservationConfirmed
            {
                ReservationId = Guid.NewGuid(),
                ScreeningId = screeningId,
                SeatIds = [Guid.NewGuid()],
                ConfirmedAt = DateTime.UtcNow
            }, CancellationToken.None));
    }
}
