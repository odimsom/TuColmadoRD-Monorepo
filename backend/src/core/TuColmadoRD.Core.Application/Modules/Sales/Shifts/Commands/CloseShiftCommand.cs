using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Commands;

public record CloseShiftCommand(
    Guid ShiftId,
    decimal ActualCashAmount,
    string? Notes
) : IRequest<OperationResult<CloseShiftResult, DomainError>>, ICommandMarker;
