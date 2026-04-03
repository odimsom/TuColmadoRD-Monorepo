using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Errors;

namespace TuColmadoRD.Core.Application.Handlers.Sync;

public class OutboxMessageDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public OutboxMessageDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<OperationResult<Unit, DomainError>> DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        var handler = message.Type switch
        {
            "SaleCreated" => _serviceProvider.GetKeyedService<TuColmadoRD.Core.Application.Interfaces.Sync.IOutboxMessageHandler>("SaleCreated"),
            "ExpenseCreated" => _serviceProvider.GetKeyedService<TuColmadoRD.Core.Application.Interfaces.Sync.IOutboxMessageHandler>("ExpenseCreated"),
            _ => null
        };

        if (handler is null)
        {
            return OperationResult<Unit, DomainError>.Bad(new SyncError("unknown_outbox_type", $"unknown_outbox_type:{message.Type}"));
        }

        return await handler.HandleAsync(message, ct);
    }
}
