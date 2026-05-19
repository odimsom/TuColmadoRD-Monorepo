using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class GetFundsQueryHandler
    : IRequestHandler<GetFundsQuery, OperationResult<IReadOnlyList<FundSummaryDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IMonetaryFundRepository _fundRepository;

    public GetFundsQueryHandler(ITenantProvider tenantProvider, IMonetaryFundRepository fundRepository)
    {
        _tenantProvider = tenantProvider;
        _fundRepository = fundRepository;
    }

    public async Task<OperationResult<IReadOnlyList<FundSummaryDto>, DomainError>> Handle(
        GetFundsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var funds = await _fundRepository.GetAllAsync(tenantId, cancellationToken);

        var dtos = funds.Select(f => new FundSummaryDto(
            f.Id,
            f.TenantId.Value,
            f.Name,
            f.CurrentBalance.Amount,
            f.CreatedAt)).ToList();

        return OperationResult<IReadOnlyList<FundSummaryDto>, DomainError>.Good(dtos);
    }
}
