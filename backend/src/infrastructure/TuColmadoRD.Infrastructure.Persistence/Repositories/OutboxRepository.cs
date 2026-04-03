using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories;

/// <summary>
/// Outbox repository implementation.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TuColmadoDbContext _dbContext;

    public OutboxRepository(TuColmadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken ct)
    {
        await _dbContext.OutboxMessages.AddAsync(outboxMessage, ct);
    }
}
