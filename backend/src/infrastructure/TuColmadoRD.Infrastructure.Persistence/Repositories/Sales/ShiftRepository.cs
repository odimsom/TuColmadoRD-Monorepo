using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public class ShiftRepository(TuColmadoDbContext dbContext)
	: GenericRepository<Shift>(dbContext), TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository, TuColmadoRD.Core.Application.Sales.Abstractions.IShiftRepository
{
	private readonly TuColmadoDbContext _dbContext = dbContext;

	public async Task<bool> HasOpenShiftAsync(Guid tenantId, Guid terminalId, CancellationToken ct)
	{
		return await _dbContext.Shifts
			.AnyAsync(x => x.TenantId.Value == tenantId && x.TerminalId == terminalId && x.Status == ShiftStatus.Open, ct);
	}

	public async Task<Shift?> GetOpenShiftAsync(Guid shiftId, Guid tenantId, CancellationToken ct)
	{
		return await _dbContext.Shifts
			.FirstOrDefaultAsync(x => x.Id == shiftId && x.TenantId.Value == tenantId && x.Status == ShiftStatus.Open, ct);
	}

	public new async Task AddAsync(Shift shift, CancellationToken ct)
	{
		await _dbContext.Shifts.AddAsync(shift, ct);
	}

	public new Task UpdateAsync(Shift shift, CancellationToken ct)
	{
	    if (_dbContext.Entry(shift).State == EntityState.Detached)
	    {
	        _dbContext.Set<Shift>().Update(shift);
	    }
	    return Task.CompletedTask;
	}
}
