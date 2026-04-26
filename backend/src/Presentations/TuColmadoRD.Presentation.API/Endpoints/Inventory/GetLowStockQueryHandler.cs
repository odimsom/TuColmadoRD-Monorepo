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
        var tenantId  = _tenantProvider.TenantId;
        var threshold = Math.Max(0, request.Threshold);

        var items = await _dbContext.Set<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()
            .AsNoTracking()
            .Where(p =>
                p.TenantId == tenantId &&
                p.IsActive &&
                p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockItemDto(
                p.Id,
                p.Name,
                p.StockQuantity,
                threshold))
            .ToListAsync(cancellationToken);

        var response = new LowStockResponse(items.Count, items.AsReadOnly());
        return OperationResult<LowStockResponse, DomainError>.Good(response);
    }
}
