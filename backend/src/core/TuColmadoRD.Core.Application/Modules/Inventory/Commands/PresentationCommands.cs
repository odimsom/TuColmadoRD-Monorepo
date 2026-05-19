using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public record DeactivatePresentationCommand(Guid PresentationId)
    : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;

public record UpdatePresentationPriceCommand(Guid PresentationId, decimal NewSalePrice, decimal NewCostPrice)
    : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;
