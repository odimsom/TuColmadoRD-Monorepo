using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Tests.Sales;

public class PaymentMethodTests
{
    [Theory]
    [InlineData(1, "Cash")]
    [InlineData(2, "Card")]
    [InlineData(3, "Transfer")]
    [InlineData(4, "Credit")]
    public void FromId_WhenKnownId_ReturnsExpectedMethod(int id, string expectedName)
    {
        var result = PaymentMethod.FromId(id);

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be(expectedName);
    }

    [Fact]
    public void FromId_WhenUnknownId_ReturnsValidationError()
    {
        var result = PaymentMethod.FromId(99);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("paymentmethod.unknown_id");
    }
}
