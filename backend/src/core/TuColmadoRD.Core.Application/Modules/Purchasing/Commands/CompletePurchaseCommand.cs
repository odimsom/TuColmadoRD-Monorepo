using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Purchasing.Commands;

public sealed record CompletePurchaseCommand(
    Guid PurchaseOrderId
) : IRequest<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>;
