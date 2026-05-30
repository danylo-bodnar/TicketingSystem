using Ticketing.Domain.Payments;

namespace Ticketing.UnitTests.Payments;

public class PricingServiceTests
{
    private readonly PricingService _sut = new();

    [Theory]
    [InlineData(1, 0.0, 100)]    // < 30%
    [InlineData(2, 0.29, 200)]   // < 30%
    [InlineData(1, 0.3, 120)]    // >= 30%, < 60%
    [InlineData(3, 0.59, 360)]   // >= 30%, < 60%
    [InlineData(2, 0.6, 300)]    // >= 60%, < 80%
    [InlineData(1, 0.79, 150)]   // >= 60%, < 80%
    [InlineData(1, 0.8, 200)]    // >= 80%
    [InlineData(5, 1.0, 1000)]   // 100%
    public void Calculate_ShouldReturnCorrectPrice(int seatCount, decimal occupancy, decimal expected)
    {
        var result = _sut.Calculate(seatCount, occupancy);

        Assert.Equal(expected, result.Amount);
        Assert.Equal("USD", result.Currency);
    }
}
