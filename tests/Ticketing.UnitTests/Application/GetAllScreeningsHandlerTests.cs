using Moq;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Handlers;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Contracts.Screenings;
using Ticketing.Domain.Screenings;

namespace Ticketing.UnitTests.Application;

public class GetAllScreeningsHandlerTests
{
    private readonly Mock<IScreeningRepository> _repoMock;
    private readonly GetAllScreeningsHandler _sut;

    public GetAllScreeningsHandlerTests()
    {
        _repoMock = new Mock<IScreeningRepository>();
        _sut = new GetAllScreeningsHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllScreenings()
    {
        var screenings = new List<Screening>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, [new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())]),
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddHours(1), [new ScreeningSeat(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())])
        };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(screenings);

        var result = await _sut.Handle(new GetAllScreeningsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(screenings[0].Id, result.Value[0].Id);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnEmptyList()
    {
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _sut.Handle(new GetAllScreeningsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
