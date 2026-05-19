using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

public sealed record StockEntryConfirmedDomainEvent(
    Guid StockEntryId,
    Guid TenantId,
    decimal TotalCost,
    int LineCount,
    DateTime OccurredAt) : IDomainEvent;
