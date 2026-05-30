namespace Ticketing.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldSetIsSuccessTrue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ShouldSetIsSuccessFalse()
    {
        var result = Result<int>.Failure("Something went wrong");

        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong", result.Error);
    }

    [Fact]
    public void GetValueOrThrow_ShouldReturnValue_WhenSuccess()
    {
        var result = Result<string>.Success("hello");

        Assert.Equal("hello", result.GetValueOrThrow());
    }

    [Fact]
    public void GetValueOrThrow_ShouldThrow_WhenFailure()
    {
        var result = Result<string>.Failure("error");

        var ex = Assert.Throws<InvalidOperationException>(() => result.GetValueOrThrow());
        Assert.Contains("error", ex.Message);
    }

    [Fact]
    public void Match_ShouldCallOnSuccess_WhenSuccess()
    {
        var result = Result<int>.Success(10);

        var output = result.Match(
            val => $"Value: {val}",
            err => $"Error: {err}"
        );

        Assert.Equal("Value: 10", output);
    }

    [Fact]
    public void Match_ShouldCallOnFailure_WhenFailure()
    {
        var result = Result<int>.Failure("fail");

        var output = result.Match(
            val => $"Value: {val}",
            err => $"Error: {err}"
        );

        Assert.Equal("Error: fail", output);
    }
}
