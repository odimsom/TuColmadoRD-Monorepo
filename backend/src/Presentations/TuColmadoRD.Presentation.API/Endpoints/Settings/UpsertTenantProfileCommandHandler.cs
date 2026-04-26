using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Presentation.API.Endpoints.Settings;

/// <summary>
/// Creates or updates the TenantProfile for the current tenant.
/// Idempotent: if a profile exists it is updated; otherwise one is created.
/// </summary>
public sealed record UpsertTenantProfileCommand(
    string BusinessName,
    string? RncValue,
    string BusinessAddress,
    string? Phone,
    string? Email)
    : IRequest<OperationResult<ResultUnit, DomainError>>;

internal sealed class UpsertTenantProfileCommandHandler
    : IRequestHandler<UpsertTenantProfileCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProfileRepository _repo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertTenantProfileCommandHandler(
        ITenantProfileRepository repo,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(
        UpsertTenantProfileCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;

        // Parse RNC only if provided
        Rnc? rnc = null;
        if (!string.IsNullOrWhiteSpace(request.RncValue))
        {
            var rncResult = Rnc.Create(request.RncValue);
            if (!rncResult.TryGetResult(out var parsedRnc))
                return OperationResult<ResultUnit, DomainError>.Bad(
                    DomainError.Validation($"RNC inválido: {rncResult.Error}"));
            rnc = parsedRnc;
        }

        var existing = await _repo.GetByTenantAsync(tenantId, cancellationToken);

        if (existing is null)
        {
            var createResult = TenantProfile.Create(
                tenantId,
                request.BusinessName,
                request.BusinessAddress,
                request.Phone,
                request.Email,
                rnc);

            if (!createResult.TryGetResult(out var newProfile))
                return OperationResult<ResultUnit, DomainError>.Bad(createResult.Error);

            await _repo.AddAsync(newProfile!, cancellationToken);
        }
        else
        {
            var updateResult = existing.Update(
                request.BusinessName,
                request.BusinessAddress,
                request.Phone,
                request.Email,
                rnc);

            if (!updateResult.IsGood)
                return updateResult;

            await _repo.UpdateAsync(existing, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
