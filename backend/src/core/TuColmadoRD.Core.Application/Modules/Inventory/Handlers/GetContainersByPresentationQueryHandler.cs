using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class GetContainersByPresentationQueryHandler
    : IRequestHandler<GetContainersByPresentationQuery, OperationResult<IReadOnlyList<ContainerDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IStockContainerRepository _containerRepository;

    public GetContainersByPresentationQueryHandler(
        ITenantProvider tenantProvider,
        IStockContainerRepository containerRepository)
    {
        _tenantProvider      = tenantProvider;
        _containerRepository = containerRepository;
    }

    public async Task<OperationResult<IReadOnlyList<ContainerDto>, DomainError>> Handle(
        GetContainersByPresentationQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var containers = await _containerRepository.GetByPresentationIdAsync(request.PresentationId, tenantId, cancellationToken);

        var dtos = containers
            .Select(c => new ContainerDto(
                c.Id,
                c.PresentationId,
                c.ContainerCode,
                c.NominalCapacity,
                c.ActualCapacity,
                c.CurrentRemaining,
                c.Status.Name,
                c.IsActiveSource,
                c.Notes,
                c.PurchasedAt,
                c.OpenedAt,
                c.EmptiedAt,
                c.CreatedAt))
            .ToList();

        return OperationResult<IReadOnlyList<ContainerDto>, DomainError>.Good(dtos);
    }
}
