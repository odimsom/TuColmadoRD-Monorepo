using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public sealed class StockContainerRepository(TuColmadoDbContext dbContext) : IStockContainerRepository
{
    public async Task<OperationResult<StockContainer, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var container = await dbContext.StockContainers
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId.Value == tenantId, ct);

        return container is null
            ? OperationResult<StockContainer, DomainError>.Bad(DomainError.NotFound("container.not_found"))
            : OperationResult<StockContainer, DomainError>.Good(container);
    }

    public async Task<IReadOnlyList<StockContainer>> GetByPresentationIdAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.StockContainers
            .Where(c => c.PresentationId == presentationId && c.TenantId.Value == tenantId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<StockContainer?> GetActiveSourceAsync(Guid presentationId, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.StockContainers
            .FirstOrDefaultAsync(c => c.PresentationId == presentationId && c.TenantId.Value == tenantId && c.IsActiveSource, ct);
    }

    public async Task AddRangeAsync(IEnumerable<StockContainer> containers, CancellationToken ct = default)
    {
        await dbContext.StockContainers.AddRangeAsync(containers, ct);
    }

    public async Task<string> NextContainerCodeAsync(Guid tenantId, CancellationToken ct = default)
    {
        var count = await dbContext.StockContainers
            .CountAsync(c => c.TenantId.Value == tenantId, ct);

        return $"C-{(count + 1):D4}";
    }
}
