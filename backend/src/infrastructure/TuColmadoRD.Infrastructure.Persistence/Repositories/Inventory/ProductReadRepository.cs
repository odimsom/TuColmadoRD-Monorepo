using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

/// <summary>
/// Product read repository implementation for query projections.
/// </summary>
public sealed class ProductReadRepository : IProductReadRepository
{
    private readonly TuColmadoDbContext _dbContext;

    public ProductReadRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id && p.TenantId == tenantId)
            .Join(
                _dbContext.Categories.AsNoTracking(),
                p => p.CategoryId,
                c => c.Id,
                (p, c) => new ProductDto(
                    p.Id,
                    p.Name,
                    p.CategoryId,
                    c.Name,
                    p.CostPrice,
                    p.SalePrice,
                    p.ItbisRate,
                    p.UnitType,
                    p.UnitType.Name,
                    p.StockQuantity,
                    p.IsActive,
                    p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(GetProductsPagedQuery query, Guid tenantId, CancellationToken ct)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 100);

        var baseQuery = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);

        if (!query.IncludeInactive)
        {
            baseQuery = baseQuery.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(query.NameFilter))
        {
            var filter = $"%{query.NameFilter.Trim()}%";
            baseQuery = baseQuery.Where(p => EF.Functions.Like(p.Name, filter));
        }

        if (query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(
                _dbContext.Categories.AsNoTracking(),
                p => p.CategoryId,
                c => c.Id,
                (p, c) => new ProductDto(
                    p.Id,
                    p.Name,
                    p.CategoryId,
                    c.Name,
                    p.CostPrice,
                    p.SalePrice,
                    p.ItbisRate,
                    p.UnitType,
                    p.UnitType.Name,
                    p.StockQuantity,
                    p.IsActive,
                    p.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, page, pageSize, totalCount);
    }
}
