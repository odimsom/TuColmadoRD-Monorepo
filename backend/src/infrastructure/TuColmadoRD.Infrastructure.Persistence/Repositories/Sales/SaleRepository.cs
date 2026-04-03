using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using AppSaleRepository = TuColmadoRD.Core.Application.Sales.Abstractions.ISaleRepository;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public class SaleRepository(TuColmadoDbContext dbContext) : GenericRepository<Sale>(dbContext), ISaleRepository, AppSaleRepository
{
	private readonly TuColmadoDbContext _dbContext = dbContext;

	public new async Task AddAsync(Sale sale, CancellationToken ct)
	{
		await _dbContext.Set<Sale>().AddAsync(sale, ct);
	}

	public async Task<Sale?> GetByIdAsync(Guid saleId, Guid tenantId, CancellationToken ct)
	{
		return await _dbContext.Set<Sale>()
			.Include(s => s.Items)
			.Include(s => s.Payments)
			.FirstOrDefaultAsync(s => s.Id == saleId && s.TenantId == tenantId, ct);
	}

	public async Task<IReadOnlyList<Sale>> GetByTerminalIdAsync(Guid terminalId, Guid tenantId, CancellationToken ct)
	{
		return await _dbContext.Set<Sale>()
			.Where(s => s.TerminalId == terminalId && s.TenantId == tenantId)
			.OrderByDescending(s => s.CreatedAt)
			.ToListAsync(ct);
	}

	public async Task<IReadOnlyList<Sale>> GetByShiftIdAsync(Guid shiftId, Guid tenantId, CancellationToken ct)
	{
		return await _dbContext.Set<Sale>()
			.Where(s => s.ShiftId == shiftId && s.TenantId == tenantId)
			.OrderByDescending(s => s.CreatedAt)
			.ToListAsync(ct);
	}

	public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetPagedAsync(
		Guid tenantId,
		int pageNumber,
		int pageSize,
		CancellationToken ct)
	{
		var query = _dbContext.Set<Sale>()
			.Where(s => s.TenantId == tenantId)
			.OrderByDescending(s => s.CreatedAt);

		var totalCount = await query.CountAsync(ct);
		var items = await query
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(ct);

		return (items, totalCount);
	}

	public new Task UpdateAsync(Sale sale, CancellationToken ct)
	{
		_dbContext.Set<Sale>().Update(sale);
		return Task.CompletedTask;
	}
}

