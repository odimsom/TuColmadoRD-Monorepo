using FluentAssertions;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Inventory;

public class TaxRateTests
{
    [Fact]
    public void Create_WhenRateIsInRange_ReturnsTaxRate()
    {
        var result = TaxRate.Create(0.18m);

        result.IsGood.Should().BeTrue();
        result.Result.Rate.Should().Be(0.18m);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Create_WhenRateIsOutOfRange_ReturnsValidationError(decimal rate)
    {
        var result = TaxRate.Create(rate);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("taxrate.out_of_range");
    }

    [Fact]
    public void CalculateTax_WhenValid_ReturnsExpectedAmount()
    {
        var rate = TaxRate.Create(0.18m).Result!;
        var baseAmount = Money.FromDecimal(100m).Result!;

        var result = rate.CalculateTax(baseAmount);

        result.IsGood.Should().BeTrue();
        result.Result.Amount.Should().Be(18m);
    }
}
