using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Outbox;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Sales.Handlers;

public sealed class VoidSaleCommandHandler : IRequestHandler<VoidSaleCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INcfAnnulmentLogRepository _annulmentLogRepository;

    public VoidSaleCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        INcfAnnulmentLogRepository annulmentLogRepository)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _annulmentLogRepository = annulmentLogRepository;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        VoidSaleCommand request,
        CancellationToken ct)
    {
        var tenantId = _tenantProvider.TenantId;
        var terminalId = _tenantProvider.TerminalId;

        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(tenantId, terminalId, ct);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
            return OperationResult<ResultUnit, DomainError>.Bad(shiftResult.Error);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, tenantId, ct);
        if (sale is null)
            return OperationResult<ResultUnit, DomainError>.Bad(DomainError.NotFound("sale.not_found"));

        if (sale.ShiftId != shift.Id)
            return OperationResult<ResultUnit, DomainError>.Bad(
                DomainError.Business("sale.wrong_shift", "Solo puedes anular ventas del turno actual."));

        var productIds = sale.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, tenantId, ct);
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var saleItem in sale.Items)
        {
            var product = productDict[saleItem.ProductId];
            var adjustResult = product.AdjustStock(saleItem.QuantityValue);
            if (!adjustResult.IsGood)
                return OperationResult<ResultUnit, DomainError>.Bad(adjustResult.Error);
        }

        var voidResult = sale.Void(request.VoidReason);
        if (!voidResult.IsGood)
            return OperationResult<ResultUnit, DomainError>.Bad(voidResult.Error);

        var reversalAmount = Money.FromDecimal(sale.TotalAmount).Result;
        var reverseResult = shift.ReverseSale(reversalAmount);
        if (!reverseResult.IsGood)
            return OperationResult<ResultUnit, DomainError>.Bad(reverseResult.Error);

        if (!string.IsNullOrWhiteSpace(sale.NcfNumber))
        {
            var annulmentLog = NcfAnnulmentLog.Create(tenantId, sale.NcfNumber, sale.Id, request.VoidReason);
            await _annulmentLogRepository.AddAsync(annulmentLog, ct);
        }

        var saleVoidedPayload = new SaleVoidedPayload(
            sale.Id,
            sale.TenantId,
            sale.TerminalId,
            sale.ReceiptNumber,
            request.VoidReason,
            DateTime.UtcNow);

        var outboxMessage = new OutboxMessage("SaleVoided", JsonSerializer.Serialize(saleVoidedPayload));

        await _saleRepository.UpdateAsync(sale, ct);
        await _productRepository.UpdateRangeAsync(products, ct);
        await _shiftRepository.UpdateAsync(shift, ct);
        await _outboxRepository.AddAsync(outboxMessage, ct);
        await _unitOfWork.CommitAsync(ct);

        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}