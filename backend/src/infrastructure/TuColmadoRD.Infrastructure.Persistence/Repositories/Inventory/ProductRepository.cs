using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using AppProductRepository = TuColmadoRD.Core.Application.Inventory.Abstractions.IProductRepository;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public class ProductRepository(TuColmadoDbContext dbContext) : GenericRepository<Product>(dbContext), IProductRepository, AppProductRepository
{
    public async Task<OperationResult<Product, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        var product = await _dbContext.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);

        return product is null
            ? OperationResult<Product, DomainError>.Bad(DomainError.NotFound("product.not_found"))
            : OperationResult<Product, DomainError>.Good(product);
    }

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        await _dbContext.Set<Product>().AddAsync(product, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyList<Guid> ids, Guid tenantId, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.Set<Product>()
            .Where(p => ids.Contains(p.Id) && p.TenantId == tenantId)
            .ToListAsync(ct);
    }

    public Task UpdateRangeAsync(IReadOnlyList<Product> products, CancellationToken ct)
    {
        _dbContext.Set<Product>().UpdateRange(products);
        return Task.CompletedTask;
    }

    public async Task<bool> CategoryExistsAsync(Guid categoryId, Guid tenantId, CancellationToken ct)
    {
        return await _dbContext.Set<Category>()
            .AnyAsync(c => c.Id == categoryId && c.TenantId == tenantId, ct);
    }
}
