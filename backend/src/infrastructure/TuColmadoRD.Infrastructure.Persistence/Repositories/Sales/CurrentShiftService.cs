using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories.Sales;

public sealed class CurrentShiftService : ICurrentShiftService
{
    private readonly IShiftReadRepository _shiftReadRepository;
    private readonly IShiftRepository _shiftRepository;

    public CurrentShiftService(IShiftReadRepository shiftReadRepository, IShiftRepository shiftRepository)
    {
        _shiftReadRepository = shiftReadRepository;
        _shiftRepository = shiftRepository;
    }

    public async Task<OperationResult<Shift, DomainError>> GetOpenShiftOrFailAsync(Guid tenantId, Guid terminalId, CancellationToken ct)
    {
        var openShiftDto = await _shiftReadRepository.GetOpenShiftByTerminalAsync(tenantId, terminalId, ct);
        if (openShiftDto is null)
        {
            return OperationResult<Shift, DomainError>.Bad(DomainError.Business("shift.no_open_shift"));
        }

        var shift = await _shiftRepository.GetOpenShiftAsync(openShiftDto.ShiftId, tenantId, ct);
        if (shift is null)
        {
            return OperationResult<Shift, DomainError>.Bad(DomainError.Business("shift.no_open_shift"));
        }

        return OperationResult<Shift, DomainError>.Good(shift);
    }
}
