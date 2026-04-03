using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

/// <summary>
/// Command to adjust product stock.
/// </summary>
public record AdjustStockCommand(
    Guid ProductId,
    decimal Delta,
    string Reason
) : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;
