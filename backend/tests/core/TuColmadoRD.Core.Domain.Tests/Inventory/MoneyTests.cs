using FluentAssertions;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Inventory;

public class MoneyTests
{
    [Fact]
    public void FromDecimal_WhenPositive_ReturnsMoney()
    {
        var result = Money.FromDecimal(125.50m);

        result.IsGood.Should().BeTrue();
        result.Result.Amount.Should().Be(125.50m);
    }

    [Fact]
    public void FromDecimal_WhenNegative_ReturnsValidationError()
    {
        var result = Money.FromDecimal(-1m);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("money.negative_value");
    }

    [Fact]
    public void Subtract_WhenInsufficientAmount_ReturnsValidationError()
    {
        var a = Money.FromDecimal(10m).Result!;
        var b = Money.FromDecimal(12m).Result!;

        var result = a - b;

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("money.insufficient_amount");
    }

    [Fact]
    public void Add_WhenValid_ReturnsSummedAmount()
    {
        var a = Money.FromDecimal(10m).Result!;
        var b = Money.FromDecimal(12m).Result!;

        var result = a + b;

        result.Amount.Should().Be(22m);
    }
}
