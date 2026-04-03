using MediatR;
using System.Text.Json;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Entities.Sales.Events;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class SaleCompletedDomainEventHandler : INotificationHandler<SaleCompletedDomainEvent>
{
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly IOutboxRepository _outboxRepository;

    public SaleCompletedDomainEventHandler(
        ICustomerAccountRepository customerAccountRepository,
        IOutboxRepository outboxRepository)
    {
        _customerAccountRepository = customerAccountRepository;
        _outboxRepository = outboxRepository;
    }

    public async Task Handle(SaleCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var creditPayments = notification.Payments
            .Where(p => p.PaymentMethodId == 4 && p.CustomerId.HasValue)
            .ToList();

        if (!creditPayments.Any())
        {
            return;
        }

        var creditByCustomer = creditPayments
            .GroupBy(p => p.CustomerId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        foreach (var (customerId, amountValue) in creditByCustomer)
        {
            var account = await _customerAccountRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            if (account is null)
            {
                throw new InvalidOperationException($"No se encontro cuenta de credito activa para el cliente ({customerId}).");
            }

            var amountResult = Money.FromDecimal(amountValue);
            if (!amountResult.TryGetResult(out var amount) || amount is null)
            {
                throw new InvalidOperationException($"El monto de credito a cobrar es invalido para el cliente {customerId}.");
            }

            string receiptRef = notification.ReceiptNumber;
            string concept = $"Fiado por venta local. Recibo: {receiptRef}";

            var chargeResult = account.RecordCharge(amount, notification.TerminalId, concept, receiptRef);
            
            if (!chargeResult.IsGood)
            {
                throw new InvalidOperationException(chargeResult.Error!);
            }

            await _customerAccountRepository.UpdateAsync(account, cancellationToken);

            foreach (var domainEvent in account.DomainEvents)
            {
                var eventType = domainEvent.GetType().Name;
                var payload = JsonSerializer.Serialize(domainEvent);
                var outboxMessage = new OutboxMessage(eventType, payload);

                await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
            }
        }
    }
}
