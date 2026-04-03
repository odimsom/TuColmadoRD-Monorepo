using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Commands;

public record OpenShiftCommand(
    decimal OpeningCashAmount,
    string CashierName
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;
