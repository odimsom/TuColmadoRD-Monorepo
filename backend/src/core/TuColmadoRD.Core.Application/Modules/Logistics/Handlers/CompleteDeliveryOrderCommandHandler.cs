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

        // Validate confirmation code (case-insensitive)
        if (!string.Equals(order.ConfirmationCode, request.ConfirmationCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(
                DomainError.Validation("delivery.invalid_code", "El código de confirmación es incorrecto."));

        // Validate GPS proximity (≤150 m required when destination has coordinates)
        if (order.Destination.Latitude.HasValue && order.Destination.Longitude.HasValue)
        {
            if (!request.DriverLatitude.HasValue || !request.DriverLongitude.HasValue)
                return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(
                    DomainError.Validation("delivery.gps_required", "Se requiere la ubicación GPS del repartidor para completar la entrega."));

            var distance = HaversineMeters(
                request.DriverLatitude.Value, request.DriverLongitude.Value,
                order.Destination.Latitude.Value, order.Destination.Longitude.Value);

            if (distance > 150)
                return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(
                    DomainError.Validation("delivery.too_far", $"El repartidor está a {distance:F0} m del destino. Debe estar a 150 m o menos."));
        }

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

    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6_371_000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
