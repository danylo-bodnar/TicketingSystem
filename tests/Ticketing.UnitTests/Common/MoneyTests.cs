using Ticketing.Domain.Common.Exceptions;
using Ticketing.Domain.Common.ValueObjects;

namespace Ticketing.UnitTests.Common;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldSetAmountAndCurrency()
    {
        var money = new Money(100, "USD");

        Assert.Equal(100, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Create_ShouldThrow_WhenAmountNegative()
    {
        var ex = Assert.Throws<InvalidMoneyAmountException>(() => new Money(-1, "USD"));
        Assert.Contains("negative", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_ShouldThrow_WhenCurrencyInvalid(string? currency)
    {
        Assert.Throws<InvalidCurrencyException>(() => new Money(100, currency!));
    }

    [Fact]
    public void Add_ShouldSumAmounts()
    {
        var a = new Money(100, "USD");
        var b = new Money(50, "USD");

        var result = a.Add(b);

        Assert.Equal(150, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_ShouldThrow_WhenCurrencyMismatch()
    {
        var a = new Money(100, "USD");
        var b = new Money(50, "EUR");

        var ex = Assert.Throws<CurrencyMismatchException>(() => a.Add(b));
        Assert.Contains("USD", ex.Message);
        Assert.Contains("EUR", ex.Message);
    }

    [Fact]
    public void Subtract_ShouldReduceAmount()
    {
        var a = new Money(100, "USD");
        var b = new Money(30, "USD");

        var result = a.Subtract(b);

        Assert.Equal(70, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenCurrencyMismatch()
    {
        var a = new Money(100, "USD");
        var b = new Money(30, "EUR");

        Assert.Throws<CurrencyMismatchException>(() => a.Subtract(b));
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultNegative()
    {
        var a = new Money(30, "USD");
        var b = new Money(100, "USD");

        var ex = Assert.Throws<InvalidMoneyAmountException>(() => a.Subtract(b));
        Assert.Contains("negative", ex.Message);
    }

    [Fact]
    public void Subtract_ShouldAllowZeroResult()
    {
        var a = new Money(50, "USD");
        var b = new Money(50, "USD");

        var result = a.Subtract(b);

        Assert.Equal(0, result.Amount);
    }

    [Fact]
    public void Equality_ShouldWorkForSameValues()
    {
        var a = new Money(100, "USD");
        var b = new Money(100, "USD");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_ShouldFailForDifferentValues()
    {
        var a = new Money(100, "USD");
        var b = new Money(200, "USD");

        Assert.NotEqual(a, b);
    }
}
