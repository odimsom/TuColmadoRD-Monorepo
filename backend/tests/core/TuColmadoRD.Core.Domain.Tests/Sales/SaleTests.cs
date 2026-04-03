using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.ValueObjects;
using SalesQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;

namespace TuColmadoRD.Tests.Sales;

public class SaleTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSale()
    {
        var result = Sale.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Caja 01",
            "REC-0001",
            "nota");

        result.IsGood.Should().BeTrue();
        result.Result.ReceiptNumber.Should().Be("REC-0001");
        result.Result.StatusId.Should().Be(SaleStatus.Completed.Id);
    }

    [Fact]
    public void Finalize_WithItemsAndPayments_RaisesSaleCompletedDomainEvent()
    {
        var sale = CreateSale().Result;

        var addItem = sale.AddItem(
            Guid.NewGuid(),
            "Arroz",
            Money.FromDecimal(100m).Result,
            Money.FromDecimal(80m).Result,
            SalesQuantity.Of(2m).Result,
            TaxRate.Create(0.18m).Result);

        addItem.IsGood.Should().BeTrue();

        var addPayment = sale.AddPayment(PaymentMethod.Cash, Money.FromDecimal(236m).Result, null);
        addPayment.IsGood.Should().BeTrue();

        var finalize = sale.Finalize();

        finalize.IsGood.Should().BeTrue();
        sale.DomainEvents.Should().ContainSingle(e => e is SaleCompletedDomainEvent);
    }

    [Fact]
    public void Finalize_WhenPaymentIsInsufficient_ReturnsBusinessError()
    {
        var sale = CreateSale().Result;

        sale.AddItem(
            Guid.NewGuid(),
            "Azucar",
            Money.FromDecimal(100m).Result,
            Money.FromDecimal(70m).Result,
            SalesQuantity.Of(1m).Result,
            TaxRate.Create(0.18m).Result);

        sale.AddPayment(PaymentMethod.Cash, Money.FromDecimal(50m).Result, null);

        var result = sale.Finalize();

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("sale.insufficient_payment");
    }

    [Fact]
    public void AddPayment_WithCreditAndNoCustomerId_ReturnsValidationError()
    {
        var sale = CreateSale().Result;

        var result = sale.AddPayment(PaymentMethod.Credit, Money.FromDecimal(100m).Result, null);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("sale.credit_payment_customer_required");
    }

    [Fact]
    public void Void_WithValidReason_ChangesStatusAndRaisesEvent()
    {
        var sale = CreateSale().Result;

        var result = sale.Void("Cliente devolvio producto");

        result.IsGood.Should().BeTrue();
        sale.StatusId.Should().Be(SaleStatus.Voided.Id);
        sale.VoidReason.Should().Be("Cliente devolvio producto");
        sale.DomainEvents.Should().ContainSingle(e => e is SaleVoidedDomainEvent);
    }

    [Fact]
    public void Void_WhenAlreadyVoided_ReturnsBusinessError()
    {
        var sale = CreateSale().Result;
        sale.Void("Primera");

        var secondVoid = sale.Void("Segunda");

        secondVoid.IsGood.Should().BeFalse();
        secondVoid.Error.Code.Should().Be("sale.already_voided");
    }

    private static TuColmadoRD.Core.Domain.Base.Result.OperationResult<Sale, TuColmadoRD.Core.Domain.ValueObjects.Base.DomainError> CreateSale()
    {
        return Sale.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Caja 02",
            "REC-TEST",
            null);
    }
}
