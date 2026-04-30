using MediatR;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Modules.Logistics.Commands;

public sealed record CompleteDeliveryOrderCommand(
    Guid DeliveryOrderId,
    IReadOnlyList<SalePaymentRequest> Payments,
    string ConfirmationCode,
    double? DriverLatitude,
    double? DriverLongitude) : IRequest<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>;
