using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public record OpenContainerCommand(
    Guid ContainerId,
    decimal? ActualCapacity
) : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;

public record DrawFromContainerCommand(
    Guid ContainerId,
    decimal Amount,
    bool AllowOverDraw = false
) : IRequest<OperationResult<decimal, DomainError>>, ICommandMarker;

public record MarkContainerEmptyCommand(
    Guid ContainerId
) : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;

public record SetActiveContainerCommand(
    Guid PresentationId,
    Guid ContainerId
) : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;
