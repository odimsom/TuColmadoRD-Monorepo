using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Tests.Sales;

public class SaleStatusTests
{
    [Theory]
    [InlineData(1, "Completed")]
    [InlineData(2, "Voided")]
    [InlineData(3, "Held")]
    public void FromId_WhenKnownId_ReturnsExpectedStatus(int id, string expectedName)
    {
        var result = SaleStatus.FromId(id);

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be(expectedName);
    }

    [Fact]
    public void FromId_WhenUnknownId_ReturnsValidationError()
    {
        var result = SaleStatus.FromId(77);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("salestatus.unknown_id");
    }
}
