using FluentAssertions;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Tests.Inventory;

public class CategoryTests
{
    [Fact]
    public void Create_WhenDataIsValid_ReturnsCategory()
    {
        var tenantId = Guid.NewGuid();

        var result = Category.Create(tenantId, "Bebidas");

        result.IsGood.Should().BeTrue();
        result.Result.Name.Should().Be("Bebidas");
        result.Result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenTenantIsEmpty_ReturnsValidationError()
    {
        var result = Category.Create(Guid.Empty, "Bebidas");

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("category.tenant_required");
    }

    [Fact]
    public void Create_WhenNameIsEmpty_ReturnsValidationError()
    {
        var result = Category.Create(Guid.NewGuid(), " ");

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("category.name_required");
    }

    [Fact]
    public void Create_WhenNameIsLongerThan80_ReturnsValidationError()
    {
        var name = new string('A', 81);

        var result = Category.Create(Guid.NewGuid(), name);

        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("category.name_too_long");
    }
}
