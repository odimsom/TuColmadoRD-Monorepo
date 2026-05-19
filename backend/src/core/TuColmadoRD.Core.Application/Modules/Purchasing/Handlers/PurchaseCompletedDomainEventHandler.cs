using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Domain.Entities.Purchasing.Events;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Application.Purchasing.Handlers;

public sealed class PurchaseCompletedDomainEventHandler : INotificationHandler<PurchaseCompletedDomainEvent>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly TuColmadoRD.Core.Application.Inventory.Abstractions.IOutboxRepository _outboxRepository;

    public PurchaseCompletedDomainEventHandler(
        IShiftRepository shiftRepository,
        TuColmadoRD.Core.Application.Inventory.Abstractions.IOutboxRepository outboxRepository)
    {
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task Handle(PurchaseCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
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
