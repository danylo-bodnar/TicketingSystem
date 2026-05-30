using System.Reflection;
using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Domain.Halls;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;

namespace Ticketing.UnitTests.Application;

public class GetAvailableSeatsHandlerTests
{
    private readonly Mock<IScreeningRepository> _screeningMock;
    private readonly Mock<IHallRepository> _hallMock;
    private readonly GetAvailableSeatsHandler _sut;

    public GetAvailableSeatsHandlerTests()
    {
        _screeningMock = new Mock<IScreeningRepository>();
        _hallMock = new Mock<IHallRepository>();
        _sut = new GetAvailableSeatsHandler(_screeningMock.Object, _hallMock.Object);
    }

    private static ScreeningSeat CreateSeatWithSeatNavigation(Guid screeningId, out Seat seatObj)
    {
        var seatId = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        seatObj = new Seat(seatId, hallId, "A", 1);
        var screeningSeat = new ScreeningSeat(Guid.NewGuid(), screeningId, seatId);
        typeof(ScreeningSeat).GetProperty("Seat")!.SetValue(screeningSeat, seatObj);
        return screeningSeat;
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableSeats()
    {
        var screeningId = Guid.NewGuid();
        var seat = CreateSeatWithSeatNavigation(screeningId, out var seatObj);
        _screeningMock.Setup(r => r.GetAvailableSeatsAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([seat]);

        var result = await _sut.Handle(new GetAvailableSeatsQuery(screeningId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal(seat.SeatId, dto.SeatId);
    }

    [Fact]
    public async Task Handle_WhenNoSeats_ShouldReturnFailure()
    {
        var screeningId = Guid.NewGuid();
        _screeningMock.Setup(r => r.GetAvailableSeatsAsync(screeningId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.Handle(new GetAvailableSeatsQuery(screeningId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("No available seats", result.Error);
    }
}
