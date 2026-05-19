using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Inventory;

public sealed class MonetaryFundRepository(TuColmadoDbContext dbContext) : IMonetaryFundRepository
{
    public async Task<OperationResult<MonetaryFund, DomainError>> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default)
    {
        var fund = await dbContext.MonetaryFunds
            .Include(f => f.Transactions)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId.Value == tenantId, ct);

        return fund is null
            ? OperationResult<MonetaryFund, DomainError>.Bad(DomainError.NotFound("fund.not_found"))
            : OperationResult<MonetaryFund, DomainError>.Good(fund);
    }

    public async Task<MonetaryFund?> GetDefaultAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.MonetaryFunds
            .OrderBy(f => f.CreatedAt)
            .FirstOrDefaultAsync(f => f.TenantId.Value == tenantId, ct);
    }

    public async Task<IReadOnlyList<MonetaryFund>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
    {
        return await dbContext.MonetaryFunds
            .Where(f => f.TenantId.Value == tenantId)
            .OrderBy(f => f.CreatedAt)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(MonetaryFund fund, CancellationToken ct = default)
    {
        await dbContext.MonetaryFunds.AddAsync(fund, ct);
    }

    public async Task TrackNewTransactionAsync(FundTransaction tx, CancellationToken ct = default)
    {
        await dbContext.Set<FundTransaction>().AddAsync(tx, ct);
    }
}
