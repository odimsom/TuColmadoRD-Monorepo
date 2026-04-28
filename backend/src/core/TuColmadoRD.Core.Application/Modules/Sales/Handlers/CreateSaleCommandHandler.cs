using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Outbox;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.System;
using TuColmadoRD.Core.Application.Interfaces.Services;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using SalesQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;
using System.Collections.Generic;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Core.Domain.Entities.Logistics;

namespace TuColmadoRD.Core.Application.Sales.Handlers;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, OperationResult<CreateSaleResult, DomainError>>
{
    private const string PrefixConsumerFinal = "B02";
    private const string PrefixCreditFiscal  = "B01";

    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSequenceService _sequenceService;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFiscalSequenceRepository _fiscalSequenceRepository;
    private readonly IFiscalReceiptRepository _fiscalReceiptRepository;
    private readonly IEcfGeneratorClient _ecfGeneratorClient;
    private readonly IEcfSignerService _ecfSignerService;
    private readonly ITenantProfileRepository _tenantProfileRepository;
    private readonly IDeliveryOrderRepository _deliveryOrderRepository;

    public CreateSaleCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        ISaleSequenceService sequenceService,
        IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        IFiscalSequenceRepository fiscalSequenceRepository,
        IFiscalReceiptRepository fiscalReceiptRepository,
        IEcfGeneratorClient ecfGeneratorClient,
        IEcfSignerService ecfSignerService,
        ITenantProfileRepository tenantProfileRepository,
        IDeliveryOrderRepository deliveryOrderRepository)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _sequenceService = sequenceService;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _fiscalSequenceRepository = fiscalSequenceRepository;
        _fiscalReceiptRepository = fiscalReceiptRepository;
        _ecfGeneratorClient = ecfGeneratorClient;
        _ecfSignerService = ecfSignerService;
        _tenantProfileRepository = tenantProfileRepository;
        _deliveryOrderRepository = deliveryOrderRepository;
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

        var isDelivery = sale.Payments.Any(p => p.PaymentMethodId == PaymentMethod.Delivery.Id);
        DeliveryOrder? deliveryOrder = null;

        if (isDelivery)
        {
            var holdResult = sale.Hold();
            if (!holdResult.IsGood)
                return OperationResult<CreateSaleResult, DomainError>.Bad(holdResult.Error);

            if (request.DeliveryAddress is null)
                return OperationResult<CreateSaleResult, DomainError>.Bad(
                    DomainError.Validation("delivery.address_required", "La dirección de entrega es requerida para pedidos de delivery."));

            var addressResult = Address.Create(
                request.DeliveryAddress.Province,
                request.DeliveryAddress.Sector,
                request.DeliveryAddress.Street,
                request.DeliveryAddress.Reference,
                request.DeliveryAddress.HouseNumber,
                request.DeliveryAddress.Latitude,
                request.DeliveryAddress.Longitude);

            if (!addressResult.TryGetResult(out var address) || address is null)
                return OperationResult<CreateSaleResult, DomainError>.Bad(DomainError.Validation("delivery.address_invalid", addressResult.Error));

            // Create DeliveryOrder (DeliveryPersonId is Guid.Empty because it's not assigned yet)
            var doResult = DeliveryOrder.Create(tenantId, sale.Id, Guid.Empty, address);
            if (!doResult.TryGetResult(out var dOrder) || dOrder is null)
                return OperationResult<CreateSaleResult, DomainError>.Bad(DomainError.Business("delivery.creation_failed", doResult.Error));

            deliveryOrder = dOrder;
        }

        var finalizeResult = sale.Finalize();
        if (!finalizeResult.IsGood)
            return OperationResult<CreateSaleResult, DomainError>.Bad(finalizeResult.Error);

        // Only register in shift if NOT delivery (payment is collected later)
        if (!isDelivery)
        {
            var registerResult = shift.RegisterSale(Money.FromDecimal(sale.TotalAmount).Result);
            if (!registerResult.IsGood)
                return OperationResult<CreateSaleResult, DomainError>.Bad(registerResult.Error);
        }

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

        var outboxMessage = new TuColmadoRD.Core.Domain.Entities.System.OutboxMessage("SaleCreated", JsonSerializer.Serialize(saleCreatedPayload));

        // ── NCF assignment ──────────────────────────────────────────────────────
        string? ncfNumber = null;
        FiscalReceipt? fiscalReceipt = null;

        var isCreditFiscal = !string.IsNullOrWhiteSpace(request.BuyerRnc);
        
        var eCfPrefix = isCreditFiscal ? "E31" : "E32";
        var bPrefix = isCreditFiscal ? PrefixCreditFiscal : PrefixConsumerFinal;

        var activeSequence = await _fiscalSequenceRepository.GetActiveByPrefixAsync(tenantId, eCfPrefix, ct)
            ?? await _fiscalSequenceRepository.GetActiveByPrefixAsync(tenantId, bPrefix, ct);

        if (activeSequence is not null)
        {
            var ncfResult = activeSequence.GetNextNcf();
            if (ncfResult.TryGetResult(out var generatedNcf) && !string.IsNullOrEmpty(generatedNcf))
            {
                ncfNumber = generatedNcf;
                sale.AssignNcf(ncfNumber);

                Rnc? buyerRnc = null;
                if (isCreditFiscal)
                {
                    var rncResult = Rnc.Create(request.BuyerRnc!);
                    if (rncResult.TryGetResult(out var parsedRnc))
                        buyerRnc = parsedRnc;
                }

                string? trackId = null;

                if (ncfNumber.StartsWith("E3", StringComparison.OrdinalIgnoreCase))
                {
                    var profile = await _tenantProfileRepository.GetByTenantAsync(tenantId, ct);
                    if (profile is not null)
                    {
                        var payload = new Dictionary<string, object>
                        {
                            ["TipoeCF"] = isCreditFiscal ? 31 : 32,
                            ["eNCF"] = ncfNumber,
                            ["RNCEmisor"] = profile.Rnc?.Value ?? "131111111",
                            ["RazonSocialEmisor"] = profile.BusinessName,
                            ["FechaEmision"] = DateTime.Now.ToString("dd-MM-yyyy"),
                            ["MontoTotal"] = sale.TotalAmount,
                            ["MontoGravadoTotal"] = sale.TotalAmount - sale.TotalItbisAmount,
                            ["TotalITBIS"] = sale.TotalItbisAmount,
                            ["IndicadorMontoGravado"] = "1",
                            ["TipoIngresos"] = "01",
                            ["TipoPago"] = "1"
                        };

                        if (isCreditFiscal)
                        {
                            payload["RNCComprador"] = request.BuyerRnc!;
                            payload["RazonSocialComprador"] = "CLIENTE CREDITO FISCAL";
                        }

                        for (int i = 0; i < saleItemResults.Count; i++)
                        {
                            payload[$"Item_{i+1}_NumeroLinea"] = i + 1;
                            payload[$"Item_{i+1}_NombreItem"] = saleItemResults[i].ProductName;
                            payload[$"Item_{i+1}_CantidadItem"] = saleItemResults[i].Quantity;
                            payload[$"Item_{i+1}_PrecioUnitarioItem"] = saleItemResults[i].UnitPrice;
                            payload[$"Item_{i+1}_MontoItem"] = saleItemResults[i].LineTotal;
                            payload[$"Item_{i+1}_IndicadorBienoServicio"] = 1;
                            payload[$"Item_{i+1}_IndicadorFacturacion"] = 1;
                        }

                        var xmlRaw = await _ecfGeneratorClient.GenerateXmlAsync(payload);
                        var signedXml = await _ecfSignerService.SignXmlAsync(xmlRaw, tenantId);

                        trackId = $"MOCK_DGII_TRACKID_{Guid.NewGuid().ToString("N")[..8]}";
                    }
                }

                fiscalReceipt = FiscalReceipt.Emit(
                    tenantId,
                    sale.Id,
                    ncfNumber,
                    Money.FromDecimal(sale.TotalItbisAmount).Result,
                    buyerRnc,
                    trackId);
            }
        }
        // ────────────────────────────────────────────────────────────────────────

        await _saleRepository.AddAsync(sale, ct);
        await _productRepository.UpdateRangeAsync(products, ct);
        await _shiftRepository.UpdateAsync(shift, ct);
        await _outboxRepository.AddAsync(outboxMessage, ct);

        if (fiscalReceipt is not null)
        {
            await _fiscalReceiptRepository.AddAsync(fiscalReceipt, ct);
            await _fiscalSequenceRepository.UpdateAsync(activeSequence!, ct);
        }

        if (deliveryOrder is not null)
        {
            await _deliveryOrderRepository.AddAsync(deliveryOrder, ct);
        }

        await _unitOfWork.CommitAsync(ct);

        return OperationResult<CreateSaleResult, DomainError>.Good(
            new CreateSaleResult(
                sale.Id,
                sale.ReceiptNumber,
                ncfNumber,
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
