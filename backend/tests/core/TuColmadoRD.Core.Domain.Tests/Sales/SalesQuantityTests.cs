using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Tests.Sales;

public class SalesQuantityTests
{
    [Fact]
    public void Of_WhenValueIsPositive_ReturnsQuantity()
    {
        var result = Quantity.Of(1.5m);

        result.IsGood.Should().BeTrue();
        result.Result.Value.Should().Be(1.5m);
    }

    [Fact]
    public void Of_WhenValueIsZero_ReturnsValidationError()
    {
        var result = Quantity.Of(0m);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("quantity.must_be_positive");
    }
}
