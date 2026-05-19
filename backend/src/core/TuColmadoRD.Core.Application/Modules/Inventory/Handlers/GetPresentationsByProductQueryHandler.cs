using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class GetPresentationsByProductQueryHandler
    : IRequestHandler<GetPresentationsByProductQuery, OperationResult<IReadOnlyList<PresentationDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;

    public GetPresentationsByProductQueryHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository)
    {
        _tenantProvider         = tenantProvider;
        _presentationRepository = presentationRepository;
    }

    public async Task<OperationResult<IReadOnlyList<PresentationDto>, DomainError>> Handle(
        GetPresentationsByProductQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var presentations = await _presentationRepository.GetByProductIdAsync(request.ProductId, tenantId, cancellationToken);

        var dtos = presentations
            .Select(p => new PresentationDto(
                p.Id,
                p.ProductId,
                p.DisplayName,
                p.PresentationType.Id,
                p.PresentationType.Name,
                p.SellMode.Id,
                p.SellMode.Name,
                (int)p.MeasureUnit,
                p.Brand,
                p.NominalCapacity,
                p.SalePrice.Amount,
                p.CostPrice.Amount,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt))
            .ToList();

        return OperationResult<IReadOnlyList<PresentationDto>, DomainError>.Good(dtos);
    }
}
