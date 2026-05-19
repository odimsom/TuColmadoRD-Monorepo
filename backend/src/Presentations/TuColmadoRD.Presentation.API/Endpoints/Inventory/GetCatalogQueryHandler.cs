using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetCatalogQueryHandler
    : IRequestHandler<GetCatalogQuery, OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>>
{
    private static readonly Dictionary<int, string> UnitLabels = new()
    {
        { 1,  "Libra"    },
        { 2,  "Onza"     },
        { 3,  "Kilogramo"},
        { 10, "Unidad"   },
        { 11, "Caja"     },
        { 12, "Paquete"  },
        { 13, "Saco"     },
        { 20, "Litro"    },
        { 21, "Galón"    },
        { 22, "Botella"  },
    };

    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCatalogQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext       = dbContext;
        _tenantProvider  = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>> Handle(
        GetCatalogQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        // 1. Active products + category names
        var products = await (from p in _dbContext.Products
                              join c in _dbContext.Categories on p.CategoryId equals c.Id
                              where p.TenantId.Value == tenantId && p.IsActive
                              orderby p.Name
                              select new
                              {
                                  p.Id,
                                  p.Name,
                                  p.CategoryId,
                                  CategoryName = c.Name,
                                  ItbisRate    = p.ItbisRate.Rate,
                              })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
            return OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>.Good(
                Array.Empty<CatalogItemDto>());

        var productIds = products.Select(p => p.Id).ToList();

        // 2. Active presentations for those products
        var presentations = await _dbContext.ProductPresentations
            .Where(pp => productIds.Contains(pp.ProductId)
                      && pp.TenantId.Value == tenantId
                      && pp.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var presentationIds = presentations.Select(pp => pp.Id).ToList();

        // 3. Packaged stock counters
        var packagedStocks = await _dbContext.PackagedStocks
            .Where(ps => presentationIds.Contains(ps.PresentationId)
                      && ps.TenantId.Value == tenantId)
            .AsNoTracking()
            .Select(ps => new { ps.PresentationId, ps.Quantity })
            .ToDictionaryAsync(ps => ps.PresentationId, ps => ps.Quantity, cancellationToken);

        // 4. Container stats — load all, filter empty in memory (ContainerStatus.Empty.Id == 3)
        var containers = await _dbContext.StockContainers
            .Where(sc => presentationIds.Contains(sc.PresentationId)
                      && sc.TenantId.Value == tenantId)
            .AsNoTracking()
            .Select(sc => new { sc.PresentationId, sc.Status, sc.CurrentRemaining })
            .ToListAsync(cancellationToken);

        var containerStats = containers
            .Where(sc => sc.Status != ContainerStatus.Empty)
            .GroupBy(sc => sc.PresentationId)
            .ToDictionary(
                g => g.Key,
                g => (Count: g.Count(), Remaining: (int)g.Sum(x => x.CurrentRemaining)));

        // 5. Build catalog grouped by product
        var presentationsByProduct = presentations
            .GroupBy(pp => pp.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var catalog = products.Select(p =>
        {
            var pres = presentationsByProduct.TryGetValue(p.Id, out var list)
                ? list
                : [];

            var presDtos = pres.Select(pp =>
            {
                var pkgQty = packagedStocks.TryGetValue(pp.Id, out var q) ? q : 0;
                containerStats.TryGetValue(pp.Id, out var cs);

                var isPackaged  = pp.PresentationType.Id == PresentationType.PackagedUnit.Id;
                var stockQty    = isPackaged ? pkgQty : cs.Remaining;
                var unitId      = (int)pp.MeasureUnit;

                return new CatalogPresentationDto(
                    pp.Id,
                    pp.ProductId,
                    pp.DisplayName,
                    pp.PresentationType.Id,
                    pp.PresentationType.Name,
                    pp.SellMode.Id,
                    pp.SellMode.Name,
                    pp.Brand,
                    pp.NominalCapacity,
                    unitId,
                    UnitLabels.TryGetValue(unitId, out var label) ? label : pp.MeasureUnit.ToString(),
                    pp.SalePrice.Amount,
                    pp.CostPrice.Amount,
                    pp.IsActive,
                    stockQty,
                    cs.Count,
                    pkgQty);
            }).ToList();

            return new CatalogItemDto(
                p.Id, p.Name, p.CategoryId, p.CategoryName, p.ItbisRate, true,
                presDtos.AsReadOnly());
        }).ToList();

        return OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>.Good(catalog);
    }
}
