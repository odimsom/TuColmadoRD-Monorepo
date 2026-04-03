using TuColmadoRD.Core.Domain.Entities.System;

namespace TuColmadoRD.Core.Application.Inventory.Abstractions;

/// <summary>
/// Outbox write abstraction for transactional integration events.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds outbox message to current transaction scope.
    /// </summary>
    Task AddAsync(OutboxMessage outboxMessage, CancellationToken ct);
}
