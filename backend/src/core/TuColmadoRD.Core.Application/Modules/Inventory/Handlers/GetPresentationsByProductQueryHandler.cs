using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class GetPresentationsByProductQueryHandler
    : IRequestHandler<GetPresentationsByProductQuery, OperationResult<IReadOnlyList<PresentationDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;
    private readonly IPackagedStockRepository _packagedStockRepository;
    private readonly IStockContainerRepository _stockContainerRepository;

    public GetPresentationsByProductQueryHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository,
        IPackagedStockRepository packagedStockRepository,
        IStockContainerRepository stockContainerRepository)
    {
        _tenantProvider           = tenantProvider;
        _presentationRepository   = presentationRepository;
        _packagedStockRepository  = packagedStockRepository;
        _stockContainerRepository = stockContainerRepository;
    }

    public async Task<OperationResult<IReadOnlyList<PresentationDto>, DomainError>> Handle(
        GetPresentationsByProductQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var presentations = await _presentationRepository.GetByProductIdAsync(request.ProductId, tenantId, cancellationToken);

        if (presentations.Count == 0)
            return OperationResult<IReadOnlyList<PresentationDto>, DomainError>.Good([]);

        var stockTasks = presentations
            .Select(p => _packagedStockRepository.GetByPresentationIdAsync(p.Id, tenantId, cancellationToken))
            .ToList();
        var containerTasks = presentations
            .Select(p => _stockContainerRepository.GetByPresentationIdAsync(p.Id, tenantId, cancellationToken))
            .ToList();

        var allStocks     = await Task.WhenAll(stockTasks);
        var allContainers = await Task.WhenAll(containerTasks);

        var dtos = presentations
            .Select((p, i) =>
            {
                var pkgQty     = allStocks[i]?.Quantity ?? 0;
                var openCount  = allContainers[i].Count(c => c.Status != ContainerStatus.Empty);

                return new PresentationDto(
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
                    p.UpdatedAt,
                    pkgQty,
                    openCount);
            })
            .ToList();

        return OperationResult<IReadOnlyList<PresentationDto>, DomainError>.Good(dtos);
    }
}
