using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Abstractions;

public interface ICurrentShiftService
{
    Task<OperationResult<Shift, DomainError>> GetOpenShiftOrFailAsync(Guid tenantId, Guid terminalId, CancellationToken ct);
}
