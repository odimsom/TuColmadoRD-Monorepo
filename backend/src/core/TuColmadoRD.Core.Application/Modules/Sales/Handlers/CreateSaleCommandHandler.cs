using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Outbox;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using SalesQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;

namespace TuColmadoRD.Core.Application.Sales.Handlers;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, OperationResult<CreateSaleResult, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSequenceService _sequenceService;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSaleCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        ISaleSequenceService sequenceService,
        IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _sequenceService = sequenceService;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<CreateSaleResult, DomainError>> Handle(
        CreateSaleCommand request,
        CancellationToken ct)
    {
        var tenantId = _tenantProvider.TenantId;
        var terminalId = _tenantProvider.TerminalId;

        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(tenantId, terminalId, ct);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
            return OperationResult<CreateSaleResult, DomainError>.Bad(shiftResult.Error);

        if (request.Items.Count == 0)
            return OperationResult<CreateSaleResult, DomainError>.Bad(
                DomainError.Validation("sale.items_required"));

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, tenantId, ct);

        var productDict = products.ToDictionary(p => p.Id);
        foreach (var itemRequest in request.Items)
        {
            if (!productDict.ContainsKey(itemRequest.ProductId))
                return OperationResult<CreateSaleResult, DomainError>.Bad(
                    DomainError.NotFound("product.not_found", $"Producto {itemRequest.ProductId} no encontrado."));
        }

        var quantitiesResult = ValidateAndPrepareQuantities(request.Items);
        if (!quantitiesResult.TryGetResult(out var quantities) || quantities is null)
            return OperationResult<CreateSaleResult, DomainError>.Bad(quantitiesResult.Error);

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            var product = productDict[item.ProductId];
            var qty = quantities[i];

            var adjustResult = product.AdjustStock(-qty.Value);
            if (!adjustResult.IsGood)
                return OperationResult<CreateSaleResult, DomainError>.Bad(adjustResult.Error);
        }

        var receiptResult = await _sequenceService.GenerateReceiptNumberAsync(tenantId, terminalId, ct);
        if (!receiptResult.TryGetResult(out var receiptNumber) || string.IsNullOrEmpty(receiptNumber))
            return OperationResult<CreateSaleResult, DomainError>.Bad(receiptResult.Error);

        var saleResult = Sale.Create(tenantId, terminalId, shift.Id, shift.CashierName, receiptNumber, request.Notes);
        if (!saleResult.TryGetResult(out var sale) || sale is null)
            return OperationResult<CreateSaleResult, DomainError>.Bad(saleResult.Error);

        var saleItemResults = new List<SaleItemResult>();
        for (int i = 0; i < request.Items.Count; i++)
        {
            var itemRequest = request.Items[i];
            var product = productDict[itemRequest.ProductId];
            var qty = quantities[i];

            var addItemResult = sale.AddItem(
                product.Id,
                product.Name,
                product.SalePrice,
                product.CostPrice,
                qty,
                product.ItbisRate);

            if (!addItemResult.IsGood)
                return OperationResult<CreateSaleResult, DomainError>.Bad(addItemResult.Error);

            saleItemResults.Add(new SaleItemResult(
                product.Id,
                product.Name,
                qty.Value,
                product.SalePrice.Amount,
                (product.SalePrice.Amount * qty.Value * product.ItbisRate.Rate),
                (product.SalePrice.Amount * qty.Value * (1 + product.ItbisRate.Rate))));
        }

        foreach (var paymentRequest in request.Payments)
        {
            var methodResult = PaymentMethod.FromId(paymentRequest.PaymentMethodId);
            if (!methodResult.TryGetResult(out var method) || method is null)
                return OperationResult<CreateSaleResult, DomainError>.Bad(methodResult.Error);

            var moneyResult = Money.FromDecimal(paymentRequest.Amount);
            if (!moneyResult.TryGetResult(out var amount) || amount is null)
                return OperationResult<CreateSaleResult, DomainError>.Bad(moneyResult.Error);

            var addPaymentResult = sale.AddPayment(method, amount, paymentRequest.Reference, paymentRequest.CustomerId);
            if (!addPaymentResult.IsGood)
                return OperationResult<CreateSaleResult, DomainError>.Bad(addPaymentResult.Error);
        }

        var finalizeResult = sale.Finalize();
        if (!finalizeResult.IsGood)
            return OperationResult<CreateSaleResult, DomainError>.Bad(finalizeResult.Error);

        var registerResult = shift.RegisterSale(Money.FromDecimal(sale.TotalAmount).Result);
        if (!registerResult.IsGood)
            return OperationResult<CreateSaleResult, DomainError>.Bad(registerResult.Error);

        var itemPayloadLines = sale.Items.Select(item => new SaleItemPayloadLine(
            item.ProductId,
            item.ProductName,
            item.QuantityValue,
            item.UnitPriceAmount,
            item.CostPriceAmount,
            item.ItbisRateValue,
            item.LineSubtotalAmount,
            item.LineItbisAmount,
            item.LineTotalAmount
        )).ToList();

        var paymentPayloadLines = sale.Payments.Select(p => new SalePaymentPayloadLine(
            p.PaymentMethodId,
            p.AmountValue,
            p.Reference,
            p.CustomerId
        )).ToList();

        var saleCreatedPayload = new SaleCreatedPayload(
            sale.Id, sale.ShiftId, sale.TenantId, sale.TerminalId, sale.ReceiptNumber,
            sale.CashierName, sale.SubtotalAmount, sale.TotalItbisAmount, sale.TotalAmount,
            sale.TotalPaidAmount, sale.ChangeDueAmount, sale.Notes, sale.CreatedAt,
            itemPayloadLines.AsReadOnly(), paymentPayloadLines.AsReadOnly());

        var outboxMessage = new OutboxMessage("SaleCreated", JsonSerializer.Serialize(saleCreatedPayload));

        await _saleRepository.AddAsync(sale, ct);
        await _productRepository.UpdateRangeAsync(products, ct);
        await _shiftRepository.UpdateAsync(shift, ct);
        await _outboxRepository.AddAsync(outboxMessage, ct);
        await _unitOfWork.CommitAsync(ct);

        return OperationResult<CreateSaleResult, DomainError>.Good(
            new CreateSaleResult(
                sale.Id,
                sale.ReceiptNumber,
                sale.SubtotalAmount,
                sale.TotalItbisAmount,
                sale.TotalAmount,
                sale.TotalPaidAmount,
                sale.ChangeDueAmount,
                saleItemResults.AsReadOnly()));
    }

    private static OperationResult<IReadOnlyList<SalesQuantity>, DomainError> ValidateAndPrepareQuantities(
        IReadOnlyList<SaleItemRequest> items)
    {
        var quantities = new List<SalesQuantity>();
        foreach (var item in items)
        {
            var qtyResult = SalesQuantity.Of(item.Quantity);
            if (!qtyResult.TryGetResult(out var qty) || qty is null)
                return OperationResult<IReadOnlyList<SalesQuantity>, DomainError>.Bad(qtyResult.Error);
            quantities.Add(qty);
        }

        return OperationResult<IReadOnlyList<SalesQuantity>, DomainError>.Good(quantities.AsReadOnly());
    }
}
