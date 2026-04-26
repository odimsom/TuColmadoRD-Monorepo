using FluentAssertions;
using Moq;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Handlers;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using SaleQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;
using Xunit;

namespace TuColmadoRD.Core.Application.Tests.Sales;

/// <summary>
/// Unit tests for the Sales application module (CreateSale and VoidSale command handlers).
/// All infrastructure is mocked via Moq — no real DB or I/O.
/// </summary>
public sealed class SalesModuleTests
{
    // ─── Mocks ────────────────────────────────────────────────────────────────
    private readonly Mock<ITenantProvider>       _tenant       = new();
    private readonly Mock<ICurrentShiftService>  _shiftService = new();
    private readonly Mock<IProductRepository>    _productRepo  = new();
    private readonly Mock<ISaleRepository>       _saleRepo     = new();
    private readonly Mock<ISaleSequenceService>  _sequence     = new();
    private readonly Mock<IShiftRepository>      _shiftRepo    = new();
    private readonly Mock<IOutboxRepository>     _outboxRepo   = new();
    private readonly Mock<IUnitOfWork>           _uow          = new();

    private readonly TenantIdentifier _tenantId = TenantIdentifier.Validate(Guid.NewGuid()).Result!;
    private readonly Guid _terminalId  = Guid.NewGuid();

    // ─── Handler factories ────────────────────────────────────────────────────
        _createSaleHandler = new CreateSaleCommandHandler(
            _tenantProviderMock.Object,
            _shiftServiceMock.Object,
            _productRepoMock.Object,
            _saleRepoMock.Object,
            _sequenceServiceMock.Object,
            _shiftRepoMock.Object,
            _outboxRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal.IFiscalSequenceRepository>().Object,
            new Mock<TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal.IFiscalReceiptRepository>().Object,
            new Mock<TuColmadoRD.Core.Application.Interfaces.Services.IEcfGeneratorClient>().Object,
            new Mock<TuColmadoRD.Core.Application.Interfaces.Services.IEcfSignerService>().Object,
            new Mock<TuColmadoRD.Core.Domain.Interfaces.Repositories.System.ITenantProfileRepository>().Object);

        _voidSaleHandler = new VoidSaleCommandHandler(
            _tenantProviderMock.Object,
            _shiftServiceMock.Object,
            _saleRepoMock.Object,
            _productRepoMock.Object,
            _shiftRepoMock.Object,
            _outboxRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal.INcfAnnulmentLogRepository>().Object);

    // ─── Setup helpers ────────────────────────────────────────────────────────
    public SalesModuleTests()
    {
        _tenant.Setup(x => x.TenantId).Returns(_tenantId);
        _tenant.Setup(x => x.TerminalId).Returns(_terminalId);

        // Default async no-ops for write methods
        _saleRepo .Setup(x => x.AddAsync   (It.IsAny<Sale>(),                    It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _saleRepo .Setup(x => x.UpdateAsync(It.IsAny<Sale>(),                    It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _productRepo.Setup(x => x.UpdateRangeAsync(It.IsAny<IReadOnlyList<Product>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _shiftRepo.Setup(x => x.UpdateAsync(It.IsAny<Shift>(),                   It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _outboxRepo.Setup(x => x.AddAsync  (It.IsAny<OutboxMessage>(),           It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    // =========================================================================
    // CreateSale tests
    // =========================================================================

    [Fact]
    public async Task CreateSale_WithValidItems_ShouldPersistSaleAndCommit()
    {
        // Arrange
        var shiftId = Guid.NewGuid();
        var shift   = BuildShift();
        var product = BuildProduct();

        _shiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(_tenantId, _terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        _sequence
            .Setup(x => x.GenerateReceiptNumberAsync(_tenantId, _terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<string, DomainError>.Good("RCP-001"));

        _productRepo
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product }.AsReadOnly());

        var command = new CreateSaleCommand(
            Items:    new List<SaleItemRequest>    { new(product.Id, 2m)            }.AsReadOnly(),
            Payments: new List<SalePaymentRequest> { new(1, 300m, null, null) }.AsReadOnly(),
            Notes:    null);

        // Act
        var result = await CreateSaleHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
        _saleRepo .Verify(x => x.AddAsync   (It.IsAny<Sale>(),    It.IsAny<CancellationToken>()), Times.Once);
        _outboxRepo.Verify(x => x.AddAsync  (It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify       (x => x.CommitAsync(It.IsAny<CancellationToken>()),                    Times.Once);
    }

    [Fact]
    public async Task CreateSale_WithoutOpenShift_ShouldReturnShiftNotFoundError()
    {
        // Arrange
        _shiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(_tenantId, _terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Bad(
                DomainError.Business("shift.not_found", "No hay turno abierto")));

        var command = new CreateSaleCommand(
            Items:    new List<SaleItemRequest>    { new(Guid.NewGuid(), 1m) }.AsReadOnly(),
            Payments: new List<SalePaymentRequest> { }.AsReadOnly(),
            Notes:    null);

        // Act
        var result = await CreateSaleHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        result.Error.Code.Should().Be("shift.not_found");
        _saleRepo.Verify(x => x.AddAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // =========================================================================
    // VoidSale tests
    // =========================================================================

    [Fact]
    public async Task VoidSale_WithValidSale_ShouldPublishOutboxAndCommit()
    {
        // Arrange
        var shift   = BuildShift();
        var product = BuildProduct();                     // mismo producto que va dentro de la venta
        var sale    = BuildCompletedSale(shift.Id, product);

        _shiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(_tenantId, _terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        _saleRepo
            .Setup(x => x.GetByIdAsync(sale.Id, _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sale);

        _productRepo
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product }.AsReadOnly());

        var command = new VoidSaleCommand(sale.Id, "Solicitud del cliente");

        // Act
        var result = await VoidSaleHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeTrue();
        _outboxRepo.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VoidSale_WhenSaleNotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var shift  = BuildShift();
        var saleId = Guid.NewGuid();

        _shiftService
            .Setup(x => x.GetOpenShiftOrFailAsync(_tenantId, _terminalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult<Shift, DomainError>.Good(shift));

        _saleRepo
            .Setup(x => x.GetByIdAsync(saleId, _tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale?)null);

        var command = new VoidSaleCommand(saleId, "Razón");

        // Act
        var result = await VoidSaleHandler.Handle(command, CancellationToken.None);

        // Assert
        result.IsGood.Should().BeFalse();
        _uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // =========================================================================
    // Domain entity fixture helpers
    // =========================================================================

    private Shift BuildShift()
    {
        var openingCash = Money.FromDecimal(0m).Result!;
        var result = Shift.Open(_tenantId, _terminalId, openingCash, "Cajero Test");
        result.IsGood.Should().BeTrue("el shift de prueba debe crearse sin errores");
        return result.Result!;
    }

    private Product BuildProduct()
    {
        var cost     = Money.FromDecimal(80m).Result!;
        var price    = Money.FromDecimal(100m).Result!;
        var taxRate  = TaxRate.Create(0.18m).Result!;
        var result   = Product.Create(_tenantId, "Producto Test", Guid.NewGuid(), cost, price, taxRate, UnitType.Unit);
        result.IsGood.Should().BeTrue("el producto de prueba debe crearse sin errores");
        var product = result.Result!;
        product.AdjustStock(100m);   // stock inicial para que las ventas puedan descontar
        return product;
    }

    /// <summary>
    /// Builds a completed Sale by going through the full domain lifecycle.
    /// </summary>
    private Sale BuildCompletedSale(Guid shiftId, Product? product = null)
    {
        product ??= BuildProduct();

        var saleResult = Sale.Create(_tenantId, _terminalId, shiftId, "Cajero Test", "RCP-TEST-001", null);
        saleResult.IsGood.Should().BeTrue();
        var sale = saleResult.Result!;

        var qty      = SaleQuantity.Of(2m).Result!;
        var cost     = Money.FromDecimal(80m).Result!;
        var price    = Money.FromDecimal(100m).Result!;
        var taxRate  = TaxRate.Create(0.18m).Result!;

        sale.AddItem(product.Id, product.Name, price, cost, qty, taxRate)
            .IsGood.Should().BeTrue();

        var paymentMethod = PaymentMethod.FromId(1).Result!;
        var amount        = Money.FromDecimal(236m).Result!;        // 100*2*1.18 = 236
        sale.AddPayment(paymentMethod, amount, null, null)
            .IsGood.Should().BeTrue();

        sale.Finalize()
            .IsGood.Should().BeTrue();

        return sale;
    }
}
