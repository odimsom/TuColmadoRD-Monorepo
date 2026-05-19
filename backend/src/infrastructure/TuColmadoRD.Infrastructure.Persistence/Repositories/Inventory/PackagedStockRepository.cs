using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public sealed class PackagedStockRepository(TuColmadoDbContext dbContext) : IPackagedStockRepository
{
    public async Task<PackagedStock?> GetByPresentationIdAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.PackagedStocks
            .FirstOrDefaultAsync(s => s.PresentationId == presentationId && s.TenantId.Value == tenantId, ct);
    }

    public async Task AddAsync(PackagedStock stock, CancellationToken ct = default)
    {
        await dbContext.PackagedStocks.AddAsync(stock, ct);
    }
}
