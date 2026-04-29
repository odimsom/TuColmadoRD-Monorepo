using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Services;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Modules.Logistics.Commands;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Logistics;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Modules.Logistics.Handlers;

public sealed class CompleteDeliveryOrderCommandHandler : IRequestHandler<CompleteDeliveryOrderCommand, OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>
{
    private readonly IDeliveryOrderRepository _deliveryRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly ICurrentShiftService _shiftService;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteDeliveryOrderCommandHandler(
        IDeliveryOrderRepository deliveryRepository,
        ISaleRepository saleRepository,
        IShiftRepository shiftRepository,
        ICurrentShiftService shiftService,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork)
    {
        _deliveryRepository = deliveryRepository;
        _saleRepository = saleRepository;
        _shiftRepository = shiftRepository;
        _shiftService = shiftService;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> Handle(CompleteDeliveryOrderCommand request, CancellationToken ct)
    {
        var tenantId = _tenantProvider.TenantId;
        var terminalId = _tenantProvider.TerminalId;

        var order = await _deliveryRepository.GetByIdAsync(request.DeliveryOrderId, cancellationToken: ct);
        if (order is null || order.TenantId.Value != tenantId.Value)
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.NotFound("delivery.order_not_found"));

        var sale = await _saleRepository.GetByIdAsync(order.SaleId, tenantId, ct);
        if (sale is null)
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.NotFound("sale.not_found"));

        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(tenantId, terminalId, ct);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(shiftResult.Error);

        // 1. Mark Delivery as Completed
        order.MarkAsDelivered();

        // 2. Replace the delivery placeholder with the real collected payment
        foreach (var p in request.Payments)
        {
            var method = PaymentMethod.FromId(p.PaymentMethodId).Result;
            var amount = Money.FromDecimal(p.Amount).Result;
            var settleResult = sale.SettleDeliveryPayment(method, amount, p.Reference, p.CustomerId);
            if (!settleResult.IsGood)
                return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(settleResult.Error);
        }

        // 3. Complete the Sale
        var completeResult = sale.Complete();
        if (!completeResult.IsGood) 
             return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(completeResult.Error);

        // 4. Register in Shift
        var registerResult = shift.RegisterSale(Money.FromDecimal(sale.TotalAmount).Result);
        if (!registerResult.IsGood) 
             return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(registerResult.Error);

        await _deliveryRepository.UpdateAsync(order, ct);
        await _saleRepository.UpdateAsync(sale, ct);
        await _shiftRepository.UpdateAsync(shift, ct);
        await _unitOfWork.CommitAsync(ct);

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
