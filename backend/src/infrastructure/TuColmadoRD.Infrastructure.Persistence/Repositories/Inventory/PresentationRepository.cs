using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public sealed class PresentationRepository(TuColmadoDbContext dbContext) : IPresentationRepository
{
    public async Task<OperationResult<ProductPresentation, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var presentation = await dbContext.ProductPresentations
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId.Value == tenantId, ct);

        return presentation is null
            ? OperationResult<ProductPresentation, DomainError>.Bad(DomainError.NotFound("presentation.not_found"))
            : OperationResult<ProductPresentation, DomainError>.Good(presentation);
    }

    public async Task<IReadOnlyList<ProductPresentation>> GetByProductIdAsync(Guid productId, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.ProductPresentations
            .Where(p => p.ProductId == productId && p.TenantId.Value == tenantId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ProductPresentation presentation, CancellationToken ct = default)
    {
        await dbContext.ProductPresentations.AddAsync(presentation, ct);
    }

    public async Task<bool> ProductExistsAsync(Guid productId, Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.Products
            .AnyAsync(p => p.Id == productId && p.TenantId.Value == tenantId, ct);
    }
}
