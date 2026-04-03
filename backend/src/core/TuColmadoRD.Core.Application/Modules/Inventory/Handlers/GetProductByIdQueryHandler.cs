using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Application.Inventory.Queries;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles product-by-id query.
/// </summary>
public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, OperationResult<ProductDto, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductReadRepository _productReadRepository;

    public GetProductByIdQueryHandler(ITenantProvider tenantProvider, IProductReadRepository productReadRepository)
    {
        _tenantProvider = tenantProvider;
        _productReadRepository = productReadRepository;
    }

    public async Task<OperationResult<ProductDto, DomainError>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;
        var product = await _productReadRepository.GetByIdAsync(request.ProductId, tenantId, cancellationToken);

        return product is null
            ? OperationResult<ProductDto, DomainError>.Bad(DomainError.NotFound("product.not_found"))
            : OperationResult<ProductDto, DomainError>.Good(product);
    }
}
