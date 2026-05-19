using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.Inventory.Events;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Tests.Inventory;

public class ProductTests
{
    [Fact]
    public void Create_WhenDataIsValid_ReturnsProductAndEmitsCreatedEvent()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var taxRate = TaxRate.Create(0.18m).Result!;

        var result = Product.Create(tenantId, "Coca Cola 2L", categoryId, taxRate);

        result.IsGood.Should().BeTrue();
        result.Result!.Name.Should().Be("Coca Cola 2L");
        result.Result.CategoryId.Should().Be(categoryId);
        result.Result.IsActive.Should().BeTrue();
        result.Result.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent);
    }

    [Fact]
    public void Create_WhenNameIsEmpty_ReturnsValidationError()
    {
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var taxRate = TaxRate.Create(0m).Result!;

        var result = Product.Create(tenantId, "   ", categoryId, taxRate);

        result.IsGood.Should().BeFalse();
        result.Error!.Code.Should().Be("product.name_required");
    }

    [Fact]
    public void Create_WhenTenantIdIsEmpty_ReturnsValidationError()
    {
        var categoryId = Guid.NewGuid();
        var taxRate = TaxRate.Create(0m).Result!;

        var result = Product.Create(Guid.Empty, "Leche", categoryId, taxRate);

        result.IsGood.Should().BeFalse();
        result.Error!.Code.Should().Be("product.tenant_required");
    }

    [Fact]
    public void UpdateName_WhenDataIsValid_UpdatesName()
    {
        var product = CreateValidProduct();

        var result = product.UpdateName("Azucar Morena");

        result.IsGood.Should().BeTrue();
        product.Name.Should().Be("Azucar Morena");
    }

    [Fact]
    public void UpdateName_WhenNameIsEmpty_ReturnsValidationError()
    {
        var product = CreateValidProduct();

        var result = product.UpdateName("   ");

        result.IsGood.Should().BeFalse();
        result.Error!.Code.Should().Be("product.name_required");
        product.Name.Should().Be("Test Product");
    }

    [Fact]
    public void Rehydrate_SetsGivenIdAndHasNoDomainEvents()
    {
        var productId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var taxRate = TaxRate.Create(0.18m).Result!;

        var product = Product.Rehydrate(productId, tenantId, "Arroz", categoryId, taxRate);

        product.Id.Should().Be(productId);
        product.Name.Should().Be("Arroz");
        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateItbisRate_WhenCalled_UpdatesRate()
    {
        var product = CreateValidProduct();
        var newRate = TaxRate.Create(0.18m).Result!;

        var result = product.UpdateItbisRate(newRate);

        result.IsGood.Should().BeTrue();
        product.ItbisRate.Should().Be(newRate);
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
        var taxRate = TaxRate.Create(0.18m).Result!;

        var created = Product.Create(tenantId, "Test Product", categoryId, taxRate);

        return created.Result!;
    }
}
