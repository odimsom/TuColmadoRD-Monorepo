using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

public interface IStockContainerRepository
{
    Task<OperationResult<StockContainer, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<StockContainer>> GetByPresentationIdAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default);
    Task<StockContainer?> GetActiveSourceAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<StockContainer> containers, CancellationToken ct = default);
    Task<string> NextContainerCodeAsync(Guid tenantId, CancellationToken ct = default);
}
