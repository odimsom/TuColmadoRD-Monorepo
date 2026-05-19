using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class AddProductPresentationCommandHandler
    : IRequestHandler<AddProductPresentationCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IPresentationRepository _presentationRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductPresentationCommandHandler(
        ITenantProvider tenantProvider,
        IPresentationRepository presentationRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider        = tenantProvider;
        _presentationRepository = presentationRepository;
        _outboxRepository      = outboxRepository;
        _unitOfWork            = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(
        AddProductPresentationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var productExists = await _presentationRepository.ProductExistsAsync(
            request.ProductId, tenantId, cancellationToken);
        if (!productExists)
            return OperationResult<Guid, DomainError>.Bad(DomainError.NotFound("product.not_found"));

        var salePriceResult = Money.FromDecimal(request.SalePrice);
        if (!salePriceResult.TryGetResult(out var salePrice))
            return OperationResult<Guid, DomainError>.Bad(salePriceResult.Error);

        var costPriceResult = Money.FromDecimal(request.CostPrice);
        if (!costPriceResult.TryGetResult(out var costPrice))
            return OperationResult<Guid, DomainError>.Bad(costPriceResult.Error);

        var presentationTypeResult = PresentationType.FromId(request.PresentationType);
        if (!presentationTypeResult.TryGetResult(out var presentationType))
            return OperationResult<Guid, DomainError>.Bad(presentationTypeResult.Error);

        var sellModeResult = SellMode.FromId(request.SellMode);
        if (!sellModeResult.TryGetResult(out var sellMode))
            return OperationResult<Guid, DomainError>.Bad(sellModeResult.Error);

        var measureUnit = (UnitOfMeasure)request.MeasureUnit;

        var presentationResult = ProductPresentation.Create(
            tenantId, request.ProductId, request.DisplayName,
            presentationType!, sellMode!, measureUnit,
            salePrice!, costPrice!, request.Brand, request.NominalCapacity);

        if (!presentationResult.TryGetResult(out var presentation))
            return OperationResult<Guid, DomainError>.Bad(presentationResult.Error);

        var outboxMessage = new OutboxMessage("PresentationCreated",
            JsonSerializer.Serialize(new
            {
                presentation!.Id,
                presentation.ProductId,
                TenantId = tenantId,
                presentation.DisplayName,
                OccurredAt = DateTime.UtcNow
            }));

        await _presentationRepository.AddAsync(presentation!, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(presentation!.Id);
    }
}
