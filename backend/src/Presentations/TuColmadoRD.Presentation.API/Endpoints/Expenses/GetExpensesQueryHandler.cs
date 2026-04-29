using MediatR;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Sales.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Treasury;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Presentation.API.Endpoints.Expenses;

internal sealed class GetExpensesQueryHandler
    : IRequestHandler<GetExpensesQuery, OperationResult<IReadOnlyList<ExpenseSummaryDto>, DomainError>>
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GetExpensesQueryHandler(TuColmadoDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<IReadOnlyList<ExpenseSummaryDto>, DomainError>> Handle(
        GetExpensesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var skip = (request.Page - 1) * request.PageSize;

        var items = await _dbContext.Set<Expense>()
            .AsNoTracking()
            .Where(e => e.TenantId.Value == tenantId)
            .OrderByDescending(e => e.Date)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(e => new ExpenseSummaryDto(
                e.Id,
                e.Amount.Amount,
                e.Category.ToString(),
                e.Description,
                e.Date))
            .ToListAsync(cancellationToken);

        return OperationResult<IReadOnlyList<ExpenseSummaryDto>, DomainError>.Good(items);
    }
}
