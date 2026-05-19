using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

public interface IMonetaryFundRepository
{
    Task<OperationResult<MonetaryFund, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<MonetaryFund?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<MonetaryFund>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(MonetaryFund fund, CancellationToken ct = default);
}
