using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.System;

namespace TuColmadoRD.Core.Application.Interfaces.Sync;

public interface IOutboxMessageHandler
{
    Task<OperationResult<Unit, DomainError>> HandleAsync(OutboxMessage message, CancellationToken ct);
}
