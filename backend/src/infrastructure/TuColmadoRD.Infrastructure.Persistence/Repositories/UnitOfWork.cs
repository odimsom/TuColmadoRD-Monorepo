using MediatR;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Infrastructure.Persistence.Contexts;

namespace TuColmadoRD.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core unit of work implementation.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TuColmadoDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(TuColmadoDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        var domainEvents = CollectDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is INotification notification)
            {
                await _mediator.Publish(notification, ct);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private IReadOnlyList<object> CollectDomainEvents()
    {
        var events = new List<object>();

        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is null)
            {
                continue;
            }

            var entityType = entry.Entity.GetType();
            var domainEventsProperty = entityType.GetProperty("DomainEvents");
            var clearMethod = entityType.GetMethod("ClearDomainEvents");

            if (domainEventsProperty is null || clearMethod is null)
            {
                continue;
            }

            if (domainEventsProperty.GetValue(entry.Entity) is not IEnumerable<object> domainEvents)
            {
                continue;
            }

            events.AddRange(domainEvents);
            clearMethod.Invoke(entry.Entity, []);
        }

        return events;
    }
}
