using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

public interface IShiftRepository
{
    Task<bool> HasOpenShiftAsync(Guid tenantId, Guid terminalId, CancellationToken ct);

    Task<Shift?> GetOpenShiftAsync(Guid shiftId, Guid tenantId, CancellationToken ct);

    Task AddAsync(Shift shift, CancellationToken ct);

    Task UpdateAsync(Shift shift, CancellationToken ct);
}
