using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

public interface IPackagedStockRepository
{
    Task<PackagedStock?> GetByPresentationIdAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(PackagedStock stock, CancellationToken ct = default);
}
