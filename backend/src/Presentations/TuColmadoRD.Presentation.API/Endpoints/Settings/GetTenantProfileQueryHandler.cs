using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.System;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Presentation.API.Endpoints.Settings;

/// <summary>
/// Returns the TenantProfile for the current tenant, or null if not yet configured.
/// </summary>
public sealed record GetTenantProfileQuery
    : IRequest<OperationResult<TenantProfileResponse?, DomainError>>;

internal sealed class GetTenantProfileQueryHandler
    : IRequestHandler<GetTenantProfileQuery, OperationResult<TenantProfileResponse?, DomainError>>
{
    private readonly ITenantProfileRepository _repo;
    private readonly ITenantProvider _tenantProvider;

    public GetTenantProfileQueryHandler(
        ITenantProfileRepository repo,
        ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<TenantProfileResponse?, DomainError>> Handle(
        GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repo.GetByTenantAsync(
            _tenantProvider.TenantId, cancellationToken);

        if (profile is null)
            return OperationResult<TenantProfileResponse?, DomainError>.Good(null);

        var dto = new TenantProfileResponse(
            profile.BusinessName,
            profile.Rnc?.Value,
            profile.BusinessAddress,
            profile.Phone,
            profile.Email);

        return OperationResult<TenantProfileResponse?, DomainError>.Good(dto);
    }
}
