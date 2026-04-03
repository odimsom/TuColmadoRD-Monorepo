using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Domain.Entities.Purchasing.Events;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Purchasing.Handlers;

public sealed class PurchaseCompletedDomainEventHandler : INotificationHandler<PurchaseCompletedDomainEvent>
{
    private readonly IProductRepository _productRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly TuColmadoRD.Core.Application.Inventory.Abstractions.IOutboxRepository _outboxRepository;

    public PurchaseCompletedDomainEventHandler(
        IProductRepository productRepository,
        IShiftRepository shiftRepository,
        TuColmadoRD.Core.Application.Inventory.Abstractions.IOutboxRepository outboxRepository)
    {
        _productRepository = productRepository;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task Handle(PurchaseCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var productsToUpdate = new List<TuColmadoRD.Core.Domain.Entities.Inventory.Product>();

        foreach (var detail in notification.Details)
        {
            var product = await _productRepository.GetByIdAsync(detail.ProductId, null, cancellationToken);
            if (product is not null)
            {
                var adjustedResult = product.AdjustStock(detail.Quantity);
                
                var moneyCostResult = Money.FromDecimal(detail.UnitCost);
                if (moneyCostResult.TryGetResult(out var costPrice) && costPrice is not null)
                {
                    product.UpdatePrice(costPrice, product.SalePrice);
                }

                productsToUpdate.Add(product);
            }
        }

        if (productsToUpdate.Any())
        {
            foreach (var p in productsToUpdate)
            {
                await ProcessDomainEventsAsync(p.DomainEvents, cancellationToken);
                await _productRepository.UpdateAsync(p, cancellationToken);
            }
        }

        var shift = await _shiftRepository.GetByIdAsync(notification.ShiftId, null, cancellationToken);
        if (shift is not null)
        {
            var totalAmountResult = Money.FromDecimal(notification.TotalAmount);
            if (totalAmountResult.TryGetResult(out var totalMoney) && totalMoney is not null)
            {
                var registerExpenseResult = shift.RegisterExpense(totalMoney);
                if (registerExpenseResult.IsGood)
                {
                    await ProcessDomainEventsAsync(shift.DomainEvents, cancellationToken);
                    await _shiftRepository.UpdateAsync(shift, cancellationToken);
                }
            }
        }
    }

    private async Task ProcessDomainEventsAsync(IEnumerable<object> events, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType().Name;
            var payload = JsonSerializer.Serialize(domainEvent);
            var outboxMessage = new OutboxMessage(eventType, payload);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }
    }
}
