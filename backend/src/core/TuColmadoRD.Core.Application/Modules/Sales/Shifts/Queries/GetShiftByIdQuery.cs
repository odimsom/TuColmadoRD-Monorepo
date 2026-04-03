using MediatR;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Queries;

public record GetShiftByIdQuery(Guid ShiftId)
    : IRequest<OperationResult<ShiftDto, DomainError>>;
