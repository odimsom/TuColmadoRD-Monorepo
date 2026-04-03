using FluentAssertions;
using Moq;
using TuColmadoRD.Core.Application.Interfaces.Repositories;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Queries;
using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using Xunit;

namespace TuColmadoRD.Core.Application.Tests.Sales;

/// <summary>
/// Integration tests for the sales module.
/// Tests create, void, and query operations with proper isolation.
/// </summary>
public sealed class SalesModuleTests
{
    private readonly Mock<ITenantProvider> _mockTenantProvider;
    private readonly Mock<ICurrentShiftService> _mockShiftService;
    private readonly Mock<ISaleRepository> _mockSaleRepository;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IShiftRepository> _mockShiftRepository;
    private readonly Mock<IOutboxRepository> _mockOutboxRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public SalesModuleTests()
    {
        _mockTenantProvider = new();
        _mockShiftService = new();
        _mockSaleRepository = new();
        _mockProductRepository = new();
        _mockShiftRepository = new();
        _mockOutboxRepository = new();
        _mockUnitOfWork = new();
    }

    private CreateSaleCommandHandler CreateSaleHandler => new(
        _mockTenantProvider.Object,
        _mockShiftService.Object,
        _mockSaleRepository.Object,
        _mockProductRepository.Object,
        _mockShiftRepository.Object,
        _mockOutboxRepository.Object,
        _mockUnitOfWork.Object);

    private VoidSaleCommandHandler VoidSaleHandler => new(
        _mockTenantProvider.Object,
        _mockShiftService.Object,
        _mockSaleRepository.Object,
        _mockProductRepository.Object,
        _mockShiftRepository.Object,
        _mockOutboxRepository.Object,
        _mockUnitOfWork.Object);

    [Fact]
    public async Task CreateSale_WithValidItems_ShouldSucceed()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        _mockTenantProvider.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantProvider.Setup(x => x.TerminalId).Returns(terminalId);

        // Mock shift service returns open shift
        var shift = ShiftFixture.CreateOpenShift(shiftId, terminalId, tenantId);
        _mockShiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(tenantId, terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        // Mock products exist with sufficient stock
        var products = CreateProductFixture(3, tenantId, 100m); // qty: 100
        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Mock repository methods
        _mockSaleRepository.Setup(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockProductRepository.Setup(x => x.UpdateRangeAsync(It.IsAny<List<Product>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockShiftRepository.Setup(x => x.UpdateAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutboxRepository.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Create command with multiple items
        var items = new List<CreateSaleItemDto>
        {
            new(products[0].Id, 5m, 100m, 18m),
            new(products[1].Id, 3m, 50m, 18m),
            new(products[2].Id, 2m, 25m, 18m)
        };

        var command = new CreateSaleCommand(
            items, "TestCustomer", new List<PaymentMethodDto>
            {
                new(1, "cash", 1234.5m, null)
            });

        var handler = CreateSaleHandler;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
        _mockSaleRepository.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSale_WithoutOpenShift_ShouldFail()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();

        _mockTenantProvider.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantProvider.Setup(x => x.TerminalId).Returns(terminalId);

        // Mock shift service returns error
        _mockShiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(tenantId, terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Bad(
                DomainError.Business("shift.not_found", "No hay turno abierto")));

        var items = new List<CreateSaleItemDto>
        {
            new(Guid.NewGuid(), 1m, 100m, 18m)
        };

        var command = new CreateSaleCommand(items, null, []);
        var handler = CreateSaleHandler;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shift.not_found");
        _mockSaleRepository.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VoidSale_WithValidSale_ShouldReverseStock()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();
        var saleId = Guid.NewGuid();

        _mockTenantProvider.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantProvider.Setup(x => x.TerminalId).Returns(terminalId);

        // Create shift and sale
        var shift = ShiftFixture.CreateOpenShift(shiftId, terminalId, tenantId);
        var sale = SaleFixture.CreateCompletedSale(saleId, shiftId, tenantId, terminalId);

        _mockShiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(tenantId, terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        _mockSaleRepository
            .Setup(x => x.GetByIdAsync(saleId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var products = CreateProductFixture(sale.Items.Count, tenantId, 50m);
        _mockProductRepository
            .Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>(), tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mockSaleRepository.Setup(x => x.UpdateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockProductRepository.Setup(x => x.UpdateRangeAsync(It.IsAny<List<Product>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockShiftRepository.Setup(x => x.UpdateAsync(It.IsAny<Shift>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutboxRepository.Setup(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new VoidSaleCommand(saleId, "Reason: Customer request");
        var handler = VoidSaleHandler;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
        _mockOutboxRepository.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VoidSale_FromWrongShift_ShouldFail()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var terminalId = Guid.NewGuid();
        var currentShiftId = Guid.NewGuid();
        var wrongShiftId = Guid.NewGuid();
        var saleId = Guid.NewGuid();

        _mockTenantProvider.Setup(x => x.TenantId).Returns(tenantId);
        _mockTenantProvider.Setup(x => x.TerminalId).Returns(terminalId);

        var shift = ShiftFixture.CreateOpenShift(currentShiftId, terminalId, tenantId);
        var sale = SaleFixture.CreateCompletedSale(saleId, wrongShiftId, tenantId, terminalId);

        _mockShiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(tenantId, terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        _mockSaleRepository
            .Setup(x => x.GetByIdAsync(saleId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        var command = new VoidSaleCommand(saleId, "Reason");
        var handler = VoidSaleHandler;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("sale.wrong_shift");
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #region Fixtures

    private static List<Product> CreateProductFixture(int count, Guid tenantId, decimal qty)
    {
        var products = new List<Product>();
        for (int i = 0; i < count; i++)
        {
            products.Add(ProductFixture.CreateProduct(tenantId, $"Product {i + 1}", qty));
        }
        return products;
    }

    #endregion
}

/// <summary>
/// Fixture factory for creating test Sale entities.
/// </summary>
internal static class SaleFixture
{
    internal static Sale CreateCompletedSale(Guid saleId, Guid shiftId, Guid tenantId, Guid terminalId)
    {
        var items = new List<SaleItem>
        {
            new(Guid.NewGuid(), "Product A", 5m, 100m, 18m),
            new(Guid.NewGuid(), "Product B", 3m, 50m, 18m)
        };

        return new Sale(
            saleId, shiftId, tenantId, terminalId,
            "RCP001", "John Doe", items, null,
            "completed", DateTime.UtcNow, null, null);
    }
}

/// <summary>
/// Fixture factory for creating test Product entities.
/// </summary>
internal static class ProductFixture
{
    internal static Product CreateProduct(Guid tenantId, string name, decimal stock)
    {
        return new Product(
            Guid.NewGuid(), tenantId, name, "SKU-001",
            Money.FromDecimal(100m).GetResult()!, 18m, stock,
            "active", DateTime.UtcNow);
    }
}

/// <summary>
/// Fixture factory for creating test Shift entities.
/// </summary>
internal static class ShiftFixture
{
    internal static Shift CreateOpenShift(Guid shiftId, Guid terminalId, Guid tenantId)
    {
        return new Shift(
            shiftId, terminalId, tenantId,
            "John Doe", DateTime.UtcNow, null,
            Money.FromDecimal(0m).GetResult()!,
            Money.FromDecimal(5000m).GetResult()!,
            "open");
    }
}
