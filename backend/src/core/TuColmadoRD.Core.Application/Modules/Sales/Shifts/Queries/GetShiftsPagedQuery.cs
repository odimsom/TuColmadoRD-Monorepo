using MediatR;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Queries;

public record GetShiftsPagedQuery(
    int Page,
    int PageSize,
    DateTime? From,
    DateTime? To,
    ShiftStatusFilter StatusFilter
) : IRequest<OperationResult<PagedResult<ShiftSummaryDto>, DomainError>>;

public enum ShiftStatusFilter
{
    All = 0,
    Open = 1,
    Closed = 2
}
