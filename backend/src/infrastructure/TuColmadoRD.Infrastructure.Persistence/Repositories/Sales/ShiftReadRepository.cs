using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Application.Sales.Shifts.Queries;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public sealed class ShiftReadRepository : IShiftReadRepository
{
    private readonly TuColmadoDbContext _dbContext;

    public ShiftReadRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShiftDto?> GetOpenShiftByTerminalAsync(Guid tenantId, Guid terminalId, CancellationToken ct)
    {
        return await _dbContext.Shifts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.TerminalId == terminalId && x.Status == ShiftStatus.Open)
            .Select(MapShiftDto())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ShiftDto?> GetByIdAsync(Guid shiftId, Guid tenantId, CancellationToken ct)
    {
        return await _dbContext.Shifts
            .AsNoTracking()
            .Where(x => x.Id == shiftId && x.TenantId == tenantId)
            .Select(MapShiftDto())
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ShiftSummaryDto>> GetPagedAsync(GetShiftsPagedQuery query, Guid tenantId, CancellationToken ct)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var source = _dbContext.Shifts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (query.From.HasValue)
        {
            source = source.Where(x => x.OpenedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            source = source.Where(x => x.OpenedAt <= query.To.Value);
        }

        source = query.StatusFilter switch
        {
            ShiftStatusFilter.Open => source.Where(x => x.Status == ShiftStatus.Open),
            ShiftStatusFilter.Closed => source.Where(x => x.Status == ShiftStatus.Closed),
            _ => source
        };

        var totalCount = await source.CountAsync(ct);

        var items = await source
            .OrderByDescending(x => x.OpenedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ShiftSummaryDto(
                x.Id,
                x.CashierName,
                x.Status.Name,
                x.OpenedAt,
                x.ClosedAt,
                x.TotalSalesCount,
                x.TotalSalesAmount.Amount,
                x.CashDifferenceAmount))
            .ToListAsync(ct);

        return new PagedResult<ShiftSummaryDto>(items, page, pageSize, totalCount);
    }

    private static Expression<Func<Shift, ShiftDto>> MapShiftDto()
    {
        return x => new ShiftDto(
            x.Id,
            x.TenantId,
            x.TerminalId,
            x.CashierName,
            x.Status.Name,
            x.OpeningCashAmount.Amount,
            x.ClosingCashAmount == null ? null : x.ClosingCashAmount.Amount,
            x.OpenedAt,
            x.ClosedAt,
            x.ExpectedCashAmount == null ? null : x.ExpectedCashAmount.Amount,
            x.ActualCashAmount == null ? null : x.ActualCashAmount.Amount,
            x.CashDifferenceAmount,
            x.Notes,
            x.TotalSalesCount,
            x.TotalSalesAmount.Amount);
    }
}
