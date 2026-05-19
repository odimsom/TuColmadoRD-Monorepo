using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetStockEntriesQueryHandler
    : IRequestHandler<GetStockEntriesQuery, OperationResult<StockEntriesPagedResult, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetStockEntriesQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<StockEntriesPagedResult, DomainError>> Handle(
        GetStockEntriesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var baseQuery = _dbContext.StockEntries
            .Where(e => e.TenantId.Value == tenantId)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var entries = await baseQuery
            .OrderByDescending(e => e.PurchasedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            var empty = new StockEntriesPagedResult([], totalCount, totalPages);
            return OperationResult<StockEntriesPagedResult, DomainError>.Good(empty);
        }

        var entryIds = entries.Select(e => e.Id).ToList();

        // Load lines with presentation display names via join
        var lines = await (from l in _dbContext.StockEntryLines
                           join pp in _dbContext.ProductPresentations on l.PresentationId equals pp.Id
                           where entryIds.Contains(l.StockEntryId)
                           select new StockEntryLineResultDto(
                               l.Id,
                               l.StockEntryId,
                               l.PresentationId,
                               pp.DisplayName,
                               l.ContainerCount,
                               l.UnitsPerContainer,
                               l.NominalSizePerUnit,
                               l.CostPerUnit.Amount,
                               l.CostPerUnit.Amount * l.ContainerCount * l.UnitsPerContainer))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var linesByEntry = lines.GroupBy(l => l.StockEntryId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<StockEntryLineResultDto>)g.ToList());

        var dtos = entries.Select(e => new StockEntryResultDto(
            e.Id,
            e.PurchasedAt,
            e.TotalCost.Amount,
            string.IsNullOrEmpty(e.SupplierName) ? null : e.SupplierName,
            string.IsNullOrEmpty(e.Notes) ? null : e.Notes,
            e.FundTransactionId,
            linesByEntry.TryGetValue(e.Id, out var ls) ? ls : [])).ToList();

        var result = new StockEntriesPagedResult(dtos, totalCount, totalPages);
        return OperationResult<StockEntriesPagedResult, DomainError>.Good(result);
    }
}
