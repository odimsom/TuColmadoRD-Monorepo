using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles product price updates.
/// </summary>
public sealed class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductRepository _productRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductPriceCommandHandler(
        ITenantProvider tenantProvider,
        IProductRepository productRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _productRepository = productRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var productResult = await _productRepository.GetByIdAsync(request.ProductId, tenantId, cancellationToken);
        if (!productResult.TryGetResult(out var product) || product is null)
        {
            return OperationResult<ResultUnit, DomainError>.Bad(productResult.Error);
        }

        var costResult = Money.FromDecimal(request.NewCostPrice);
        if (!costResult.TryGetResult(out var costPrice) || costPrice is null)
        {
            return OperationResult<ResultUnit, DomainError>.Bad(costResult.Error);
        }

        var saleResult = Money.FromDecimal(request.NewSalePrice);
        if (!saleResult.TryGetResult(out var salePrice) || salePrice is null)
        {
            return OperationResult<ResultUnit, DomainError>.Bad(saleResult.Error);
        }

        var updateResult = product.UpdatePrice(costPrice, salePrice);
        if (!updateResult.IsGood)
        {
            return updateResult;
        }

        var payload = new ProductPriceUpdatedPayload(
            product.Id,
            tenantId,
            product.CostPrice,
            product.SalePrice,
            product.UpdatedAt);

        var outboxMessage = new OutboxMessage("ProductPriceUpdated", JsonSerializer.Serialize(payload));
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
