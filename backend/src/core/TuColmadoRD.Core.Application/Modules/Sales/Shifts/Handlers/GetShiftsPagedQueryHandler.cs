using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;
using TuColmadoRD.Core.Application.Sales.Shifts.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Shifts.Handlers;

public sealed class GetShiftsPagedQueryHandler : IRequestHandler<GetShiftsPagedQuery, OperationResult<PagedResult<ShiftSummaryDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IShiftReadRepository _shiftReadRepository;

    public GetShiftsPagedQueryHandler(ITenantProvider tenantProvider, IShiftReadRepository shiftReadRepository)
    {
        _tenantProvider = tenantProvider;
        _shiftReadRepository = shiftReadRepository;
    }

    public async Task<OperationResult<PagedResult<ShiftSummaryDto>, DomainError>> Handle(GetShiftsPagedQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var paged = await _shiftReadRepository.GetPagedAsync(request, tenantId, cancellationToken);
        return OperationResult<PagedResult<ShiftSummaryDto>, DomainError>.Good(paged);
    }
}
