using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Inventory.Handlers;

internal sealed class GetFundTransactionsQueryHandler
    : IRequestHandler<GetFundTransactionsQuery, OperationResult<FundTransactionsPagedResult, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetFundTransactionsQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<FundTransactionsPagedResult, DomainError>> Handle(
        GetFundTransactionsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var page     = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var fundExists = await _dbContext.MonetaryFunds
            .AnyAsync(f => f.Id == request.FundId && f.TenantId.Value == tenantId, cancellationToken);

        if (!fundExists)
            return OperationResult<FundTransactionsPagedResult, DomainError>.Bad(
                DomainError.NotFound("fund.not_found"));

        var baseQuery = _dbContext.FundTransactions
            .Where(t => t.FundId == request.FundId && t.TenantId.Value == tenantId)
            .AsNoTracking();

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await baseQuery
            .OrderByDescending(t => t.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(t => new FundTransactionPagedDto(
            t.Id,
            t.FundId,
            t.Type.Id,
            t.Type.Name,
            t.Amount.Amount,
            t.Category?.Id,
            t.Category?.Name,
            t.Description,
            t.JustificationNote,
            t.ReferenceId,
            t.BalanceAfter.Amount,
            t.OccurredAt)).ToList();

        var result = new FundTransactionsPagedResult(dtos, page, pageSize, totalCount, totalPages);
        return OperationResult<FundTransactionsPagedResult, DomainError>.Good(result);
    }
}
