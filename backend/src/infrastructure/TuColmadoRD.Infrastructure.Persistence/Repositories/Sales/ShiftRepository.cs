using Microsoft.EntityFrameworkCore;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Infrastructure.Persistence.Contexts;
using TuColmadoRD.Infrastructure.Persistence.Repositories.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public class ShiftRepository(TuColmadoDbContext dbContext)
	: GenericRepository<Shift>(dbContext), TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository, TuColmadoRD.Core.Application.Sales.Abstractions.IShiftRepository
{
	private readonly TuColmadoDbContext _context = dbContext;

	public async Task<bool> HasOpenShiftAsync(Guid tenantId, Guid terminalId, CancellationToken ct)
	{
		return await _context.Shifts
			.AnyAsync(x => x.TenantId.Value == tenantId && x.TerminalId == terminalId && x.Status == ShiftStatus.Open, ct);
	}

	public async Task<Shift?> GetOpenShiftAsync(Guid shiftId, Guid tenantId, CancellationToken ct)
	{
		return await _context.Shifts
			.FirstOrDefaultAsync(x => x.Id == shiftId && x.TenantId.Value == tenantId && x.Status == ShiftStatus.Open, ct);
	}

	public new async Task AddAsync(Shift shift, CancellationToken ct)
	{
		await _context.Shifts.AddAsync(shift, ct);
	}

	public new Task UpdateAsync(Shift shift, CancellationToken ct)
	{
	    if (_context.Entry(shift).State == EntityState.Detached)
	    {
	        _context.Set<Shift>().Update(shift);
	    }
	    return Task.CompletedTask;
	}
}
