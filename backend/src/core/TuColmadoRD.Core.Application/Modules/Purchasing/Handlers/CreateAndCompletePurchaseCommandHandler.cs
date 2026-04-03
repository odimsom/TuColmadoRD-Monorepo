using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Purchasing.Commands;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Purchasing;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Purchasing.Handlers;

internal sealed class CreateAndCompletePurchaseCommandHandler : IRequestHandler<CreateAndCompletePurchaseCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAndCompletePurchaseCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        IPurchaseOrderRepository purchaseOrderRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _purchaseOrderRepository = purchaseOrderRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(CreateAndCompletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        
        // Using a hardcoded TerminalId as a placeholder or assuming the user context provides it
        // We will pass empty Guid to get the shift or assumption depends on current shift service implementation
        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(tenantId.Value, Guid.Empty, cancellationToken);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
        {
            return OperationResult<Guid, DomainError>.Bad(shiftResult.Error);
        }

        var orderResult = PurchaseOrder.Create(tenantId, request.SupplierId, shift.Id, request.SupplierNcf);
        if (!orderResult.TryGetResult(out var order) || order is null)
        {
            return OperationResult<Guid, DomainError>.Bad(orderResult.Error);
        }

        foreach (var item in request.Items)
        {
            order.AddDetail(item.ProductId, item.Quantity, item.UnitCost);
        }

        var completeResult = order.CompletePurchase();
        if (!completeResult.IsGood)
        {
            return OperationResult<Guid, DomainError>.Bad(completeResult.Error);
        }

        await _purchaseOrderRepository.AddAsync(order, cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
        {
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent);
            var outboxMessage = new OutboxMessage(eventType, payload);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(order.Id);
    }
}
