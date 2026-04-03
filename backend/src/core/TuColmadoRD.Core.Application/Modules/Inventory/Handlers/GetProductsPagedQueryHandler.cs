using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles products paged query.
/// </summary>
public sealed class GetProductsPagedQueryHandler : IRequestHandler<GetProductsPagedQuery, OperationResult<PagedResult<ProductDto>, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductReadRepository _productReadRepository;

    public GetProductsPagedQueryHandler(ITenantProvider tenantProvider, IProductReadRepository productReadRepository)
    {
        _tenantProvider = tenantProvider;
        _productReadRepository = productReadRepository;
    }

    public async Task<OperationResult<PagedResult<ProductDto>, DomainError>> Handle(GetProductsPagedQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var pageResult = await _productReadRepository.GetPagedAsync(request, tenantId, cancellationToken);
        return OperationResult<PagedResult<ProductDto>, DomainError>.Good(pageResult);
    }
}
