using System.Reflection;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Handlers;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;

namespace Ticketing.UnitTests.Application;

public class GetScreeningSeatsHandlerTests
{
    private readonly Mock<IScreeningRepository> _repoMock;
    private readonly GetScreeningSeatsHandler _sut;

    public GetScreeningSeatsHandlerTests()
    {
        _repoMock = new Mock<IScreeningRepository>();
        _sut = new GetScreeningSeatsHandler(_repoMock.Object);
    }

    private static ScreeningSeat CreateSeatWithSeatNavigation(Guid screeningId, string row, int column)
    {
        var seatId = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        var seatObj = new Seat(seatId, hallId, row, column);
        var screeningSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, seatId);
        typeof(ScreeningSeat).GetProperty("Seat")!.SetValue(screeningSeat, seatObj);
        return screeningSeat;
    }

    [Fact]
    public async Task Handle_ShouldReturnAllSeats()
    {
        var screeningId = Guid.NewGuid();
        var seat1 = CreateSeatWithSeatNavigation(screeningId, "A", 1);
        var seat2 = CreateSeatWithSeatNavigation(screeningId, "B", 1);
        var screening = new Screening(screeningId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [seat1, seat2]);
        _repoMock.Setup(r => r.GetSeats(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync(screening);

        var result = await _sut.Handle(new GetScreeningSeatsQuery(screeningId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Handle_WhenScreeningNotFound_ShouldReturnFailure()
    {
        var screeningId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetSeats(screeningId, It.IsAny<CancellationToken>())).ReturnsAsync((Screening?)null);

        var result = await _sut.Handle(new GetScreeningSeatsQuery(screeningId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Error);
    }
}
