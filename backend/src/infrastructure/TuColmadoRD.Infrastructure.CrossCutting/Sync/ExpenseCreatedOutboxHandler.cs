using TuColmadoRD.Core.Application.Interfaces.Sync;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Infrastructure.CrossCutting.Sync;

public class ExpenseCreatedOutboxHandler : IOutboxMessageHandler
{
    public Task<OperationResult<Unit, DomainError>> HandleAsync(OutboxMessage message, CancellationToken ct)
    {
        var result = OperationResult<Unit, DomainError>.Bad(
            new SyncError("TransientFailure", "transient:expense_sync_not_implemented"));

        return Task.FromResult(result);
    }
}
