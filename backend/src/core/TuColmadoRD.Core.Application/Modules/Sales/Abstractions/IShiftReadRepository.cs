using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Application.Sales.Shifts.Queries;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

public interface IShiftReadRepository
{
    Task<ShiftDto?> GetOpenShiftByTerminalAsync(Guid tenantId, Guid terminalId, CancellationToken ct);

    Task<ShiftDto?> GetByIdAsync(Guid shiftId, Guid tenantId, CancellationToken ct);

    Task<PagedResult<ShiftSummaryDto>> GetPagedAsync(GetShiftsPagedQuery query, Guid tenantId, CancellationToken ct);
}
