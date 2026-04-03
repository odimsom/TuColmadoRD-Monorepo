using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using SalesQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;

namespace TuColmadoRD.Tests.Sales;

public class SaleEntitiesTests
{
    [Fact]
    public void SaleItem_WhenCreated_ComputesTotals()
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            "Leche",
            Money.FromDecimal(80m).Result,
            Money.FromDecimal(60m).Result,
            SalesQuantity.Of(2m).Result,
            TaxRate.Create(0.18m).Result);

        item.LineSubtotalAmount.Should().Be(160m);
        item.LineItbisAmount.Should().Be(28.8m);
        item.LineTotalAmount.Should().Be(188.8m);
    }

    [Fact]
    public void SalePayment_WhenCreated_SetsPaymentMethodAndAmount()
    {
        var payment = new SalePayment(PaymentMethod.Card, 250m, "****1234", null);

        payment.PaymentMethodId.Should().Be(PaymentMethod.Card.Id);
        payment.AmountValue.Should().Be(250m);
        payment.Reference.Should().Be("****1234");
    }
}
