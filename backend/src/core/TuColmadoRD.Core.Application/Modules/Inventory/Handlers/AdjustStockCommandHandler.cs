using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles stock adjustment command.
/// </summary>
public sealed class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductRepository _productRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStockCommandHandler(
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

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var productResult = await _productRepository.GetByIdAsync(request.ProductId, tenantId, cancellationToken);
        if (!productResult.TryGetResult(out var product) || product is null)
        {
            return OperationResult<ResultUnit, DomainError>.Bad(productResult.Error);
        }

        var adjustResult = product.AdjustStock(request.Delta);
        if (!adjustResult.IsGood)
        {
            return adjustResult;
        }

        var payload = new StockAdjustedPayload(
            product.Id,
            tenantId,
            request.Delta,
            product.StockQuantity,
            request.Reason,
            product.UpdatedAt);

        var outboxMessage = new OutboxMessage("StockAdjusted", JsonSerializer.Serialize(payload));
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
