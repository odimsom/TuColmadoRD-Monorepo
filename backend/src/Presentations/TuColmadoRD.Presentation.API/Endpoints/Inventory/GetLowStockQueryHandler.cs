using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetLowStockQueryHandler
    : IRequestHandler<GetLowStockQuery, OperationResult<LowStockResponse, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetLowStockQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<LowStockResponse, DomainError>> Handle(
        GetLowStockQuery request, CancellationToken cancellationToken)
    {
        var tenantId  = (Guid)_tenantProvider.TenantId;
        var threshold = Math.Max(0, request.Threshold);

        var items = await (from ps in _dbContext.PackagedStocks
                           join pp in _dbContext.ProductPresentations on ps.PresentationId equals pp.Id
                           join p in _dbContext.Products on pp.ProductId equals p.Id
                           where ps.TenantId.Value == tenantId
                              && pp.IsActive
                              && p.IsActive
                              && ps.Quantity <= threshold
                           orderby ps.Quantity
                           select new LowStockItemDto(
                               ps.PresentationId,
                               p.Id,
                               p.Name,
                               pp.DisplayName,
                               ps.Quantity,
                               threshold))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = new LowStockResponse(items.Count, items.AsReadOnly());
        return OperationResult<LowStockResponse, DomainError>.Good(response);
    }
}
