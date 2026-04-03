using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Purchasing.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Purchasing.Handlers;

public sealed class CompletePurchaseCommandHandler : IRequestHandler<CompletePurchaseCommand, OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompletePurchaseCommandHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> Handle(CompletePurchaseCommand request, CancellationToken cancellationToken)
    {
        var order = await _purchaseOrderRepository.GetByIdAsync(request.PurchaseOrderId, new System.Linq.Expressions.Expression<Func<TuColmadoRD.Core.Domain.Entities.Purchasing.PurchaseOrder, object>>[] { x => x.Details }, cancellationToken);
        if (order is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.NotFound("purchase.not_found", "La orden de compra no existe."));
        }

        var completeResult = order.CompletePurchase();
        if (!completeResult.IsGood)
        {
            return completeResult;
        }

        foreach (var domainEvent in order.DomainEvents)
        {
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent);
            var outboxMessage = new OutboxMessage(eventType, payload);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        await _purchaseOrderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
