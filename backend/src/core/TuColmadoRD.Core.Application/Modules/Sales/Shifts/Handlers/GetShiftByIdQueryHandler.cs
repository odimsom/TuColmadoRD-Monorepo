using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Application.Sales.Shifts.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Handlers;

public sealed class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, OperationResult<ShiftDto, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IShiftReadRepository _shiftReadRepository;

    public GetShiftByIdQueryHandler(ITenantProvider tenantProvider, IShiftReadRepository shiftReadRepository)
    {
        _tenantProvider = tenantProvider;
        _shiftReadRepository = shiftReadRepository;
    }

    public async Task<OperationResult<ShiftDto, DomainError>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var dto = await _shiftReadRepository.GetByIdAsync(request.ShiftId, tenantId, cancellationToken);

        if (dto is null)
        {
            return OperationResult<ShiftDto, DomainError>.Bad(DomainError.NotFound("shift.not_found"));
        }

        return OperationResult<ShiftDto, DomainError>.Good(dto);
    }
}
