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
        var tenantId = _tenantProvider.TenantId;
        
        var products = await _dbContext.Set<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .Select(p => new CatalogItemDto(
                p.Id,
                p.Name,
                p.SalePrice.Amount,
                p.StockQuantity,
                p.UnitType.Id))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>.Good(products);
    }
}
