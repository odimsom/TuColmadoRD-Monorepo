using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetCatalogQueryHandler : IRequestHandler<GetCatalogQuery, OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetCatalogQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>> Handle(GetCatalogQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var products = await (from p in _dbContext.Products
                              join c in _dbContext.Categories on p.CategoryId equals c.Id
                              where p.TenantId.Value == tenantId && p.IsActive
                              select new CatalogItemDto(
                                  p.Id,
                                  p.Name,
                                  p.CategoryId,
                                  c.Name,
                                  p.SalePrice.Amount,
                                  p.StockQuantity,
                                  p.ItbisRate.Rate,
                                  p.UnitType.Id,
                                  p.IsActive))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>.Good(products);
    }
}
