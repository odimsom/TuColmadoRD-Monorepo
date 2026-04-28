using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Modules.Logistics.Commands;

public sealed record AcceptDeliveryOrderCommand(Guid DeliveryOrderId, Guid DeliveryPersonId) : IRequest<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>;
