using MediatR;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Sales.Commands;

/// <summary>
/// Command to void a completed sale.
/// </summary>
public sealed record VoidSaleCommand(
    Guid SaleId,
    string VoidReason
) : IRequest<OperationResult<ResultUnit, DomainError>>, ICommandMarker;
