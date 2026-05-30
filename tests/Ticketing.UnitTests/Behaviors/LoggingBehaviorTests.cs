using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Ticketing.Application.Behaviors;

namespace Ticketing.UnitTests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<SampleRequest, Result<string>>>> _loggerMock;
    private readonly LoggingBehavior<SampleRequest, Result<string>> _sut;

    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<SampleRequest, Result<string>>>>();
        _sut = new LoggingBehavior<SampleRequest, Result<string>>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSuccess_ShouldLogInformation()
    {
        var response = Result<string>.Success("ok");

        var result = await _sut.Handle(
            new SampleRequest(),
            _ => Task.FromResult(response),
            CancellationToken.None);

        Assert.Same(response, result);
        VerifyLog(LogLevel.Information, "Completed");
    }

    [Fact]
    public async Task Handle_WhenResultFailure_ShouldLogWarning()
    {
        var response = Result<string>.Failure("something went wrong");

        var result = await _sut.Handle(
            new SampleRequest(),
            _ => Task.FromResult(response),
            CancellationToken.None);

        Assert.Same(response, result);
        VerifyLog(LogLevel.Warning, "Failed");
    }

    [Fact]
    public async Task Handle_WhenException_ShouldLogErrorAndRethrow()
    {
        var ex = new InvalidOperationException("test error");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.Handle(
                new SampleRequest(),
                _ => throw ex,
                CancellationToken.None));

        Assert.Same(ex, exception);
        VerifyLog(LogLevel.Error, "Exception");
    }

    private void VerifyLog(LogLevel level, string contains)
    {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(contains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    public record SampleRequest : IRequest<Result<string>>;
}
