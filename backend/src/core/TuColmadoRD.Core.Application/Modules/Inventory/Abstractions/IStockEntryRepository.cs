using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

public interface IStockEntryRepository
{
    Task AddAsync(StockEntry entry, CancellationToken ct = default);
}
