using FluentAssertions;
using TuColmadoRD.Core.Domain.Enums.Sales;

namespace TuColmadoRD.Tests.Sales;

public class ShiftStatusTests
{
    [Theory]
    [InlineData(1, "Open")]
    [InlineData(2, "Closed")]
    public void FromId_WhenKnownId_ReturnsExpectedStatus(int id, string expectedName)
    {
        var result = ShiftStatus.FromId(id);

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be(expectedName);
    }

    [Fact]
    public void FromId_WhenUnknownId_ReturnsValidationError()
    {
        var result = ShiftStatus.FromId(99);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shiftstatus.unknown_id");
    }
}
