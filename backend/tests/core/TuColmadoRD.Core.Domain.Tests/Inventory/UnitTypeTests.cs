using FluentAssertions;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

namespace TuColmadoRD.Tests.Inventory;

public class UnitTypeTests
{
    [Theory]
    [InlineData(1, "Unit")]
    [InlineData(2, "Pound")]
    [InlineData(3, "Liter")]
    [InlineData(4, "Box")]
    public void FromId_WhenKnownId_ReturnsExpectedUnit(int id, string expectedName)
    {
        var result = UnitType.FromId(id);

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be(expectedName);
    }

    [Fact]
    public void FromId_WhenUnknownId_ReturnsValidationError()
    {
        var result = UnitType.FromId(99);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("unittype.unknown_id");
    }
}
