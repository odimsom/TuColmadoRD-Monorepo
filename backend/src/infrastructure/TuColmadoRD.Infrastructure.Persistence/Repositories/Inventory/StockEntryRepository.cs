using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public sealed class StockEntryRepository(TuColmadoDbContext dbContext) : IStockEntryRepository
{
    public async Task AddAsync(StockEntry entry, CancellationToken ct = default)
    {
        await dbContext.StockEntries.AddAsync(entry, ct);
    }
}
