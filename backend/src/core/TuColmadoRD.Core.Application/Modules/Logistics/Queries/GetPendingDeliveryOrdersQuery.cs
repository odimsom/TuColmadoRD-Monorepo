using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Modules.Logistics.Queries;

public sealed record DeliveryOrderDto(
    Guid Id,
    Guid SaleId,
    string ReceiptNumber,
    decimal TotalAmount,
    string CustomerName,
    string Phone,
    string AddressProvince,
    string AddressSector,
    string AddressStreet,
    string? AddressHouseNumber,
    string AddressReference,
    double? Latitude,
    double? Longitude,
    string Status);

public sealed record GetPendingDeliveryOrdersQuery() : IRequest<OperationResult<IReadOnlyList<DeliveryOrderDto>, DomainError>>;
