using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles product creation command.
/// </summary>
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductRepository _productRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
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

    public async Task<OperationResult<Guid, DomainError>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var taxResult = TaxRate.Create(request.ItbisRate);
        if (!taxResult.TryGetResult(out var itbisRate) || itbisRate is null)
        {
            return OperationResult<Guid, DomainError>.Bad(taxResult.Error);
        }

        var categoryExists = await _productRepository.CategoryExistsAsync(request.CategoryId, tenantId, cancellationToken);
        if (!categoryExists)
        {
            return OperationResult<Guid, DomainError>.Bad(DomainError.NotFound("category.not_found"));
        }

        var productResult = Product.Create(tenantId, request.Name, request.CategoryId, itbisRate!);
        if (!productResult.TryGetResult(out var product) || product is null)
        {
            return OperationResult<Guid, DomainError>.Bad(productResult.Error);
        }

        var payload = new ProductCreatedPayload(
            product.Id,
            tenantId,
            product.Name,
            product.CategoryId,
            product.ItbisRate.Rate,
            product.CreatedAt);

        var outboxMessage = new OutboxMessage("ProductCreated", JsonSerializer.Serialize(payload));

        await _productRepository.AddAsync(product, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(product.Id);
    }
}
