using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Inventory;

public class ProductTests
{
    [Fact]
    public void Create_WhenDataIsValid_ReturnsProductAndEmitsCreatedEvent()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var cost = Money.FromDecimal(50m).Result!;
        var sale = Money.FromDecimal(75m).Result!;
        var taxRate = TaxRate.Create(0.18m).Result!;

        var result = Product.Create(tenantId, "Coca Cola 2L", categoryId, cost, sale, taxRate, UnitType.Unit);

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be("Coca Cola 2L");
        result.Result.CategoryId.Should().Be(categoryId);
        result.Result.StockQuantity.Should().Be(0m);
        result.Result.IsActive.Should().BeTrue();
        result.Result.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Create_WhenSalePriceIsBelowCost_ReturnsValidationError()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var cost = Money.FromDecimal(80m).Result!;
        var sale = Money.FromDecimal(70m).Result!;
        var taxRate = TaxRate.Create(0.18m).Result!;

        var result = Product.Create(tenantId, "Leche", categoryId, cost, sale, taxRate, UnitType.Liter);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("product.sale_price_below_cost");
    }

    [Fact]
    public void UpdatePrice_WhenDataIsValid_UpdatesPricesAndEmitsEvent()
    {
        var product = CreateValidProduct();
        var newCost = Money.FromDecimal(60m).Result!;
        var newSale = Money.FromDecimal(95m).Result!;

        var result = product.UpdatePrice(newCost, newSale);

        result.IsGood.Should().BeTrue();
        product.CostPrice.Amount.Should().Be(60m);
        product.SalePrice.Amount.Should().Be(95m);
        product.DomainEvents.Should().Contain(e => e is ProductPriceUpdatedDomainEvent);
    }

    [Fact]
    public void AdjustStock_WhenResultWouldBeNegative_ReturnsBusinessError()
    {
        var product = CreateValidProduct();

        var result = product.AdjustStock(-1m);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("product.insufficient_stock");
        product.StockQuantity.Should().Be(0m);
    }

    [Fact]
    public void RehydrateForCatalogSync_SetsGivenIdAndClearsDomainEvents()
    {
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var cost = Money.FromDecimal(40m).Result!;
        var sale = Money.FromDecimal(65m).Result!;
        var taxRate = TaxRate.Create(0.18m).Result!;

        var result = Product.RehydrateForCatalogSync(productId, tenantId, categoryId, "Arroz", cost, sale, taxRate);

        result.IsGood.Should().BeTrue();
        result.Result.Id.Should().Be(productId);
        result.Result.UnitType.Should().Be(UnitType.Unit);
        result.Result.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateFromCatalogSync_WhenDataIsValid_UpdatesCoreFields()
    {
        var product = CreateValidProduct();
        var newCategoryId = Guid.NewGuid();
        var newCost = Money.FromDecimal(25m).Result!;
        var newSale = Money.FromDecimal(38m).Result!;

        var result = product.UpdateFromCatalogSync(newCategoryId, "  Azucar Morena  ", newCost, newSale);

        result.IsGood.Should().BeTrue();
        product.Name.Should().Be("Azucar Morena");
        product.CategoryId.Should().Be(newCategoryId);
        product.CostPrice.Amount.Should().Be(25m);
        product.SalePrice.Amount.Should().Be(38m);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalseAndEmitsEvent()
    {
        var product = CreateValidProduct();

        product.Deactivate();

        product.IsActive.Should().BeFalse();
        product.DomainEvents.Should().Contain(e => e is ProductDeactivatedDomainEvent);
    }

    private static Product CreateValidProduct()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var cost = Money.FromDecimal(10m).Result!;
        var sale = Money.FromDecimal(15m).Result!;
        var taxRate = TaxRate.Create(0.18m).Result!;

        var created = Product.Create(tenantId, "Test Product", categoryId, cost, sale, taxRate, UnitType.Unit);

        return created.Result!;
    }
}
