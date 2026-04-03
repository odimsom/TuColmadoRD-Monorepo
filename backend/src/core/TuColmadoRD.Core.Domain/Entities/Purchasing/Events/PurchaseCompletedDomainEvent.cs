using MediatR;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Purchasing.Events;

public sealed record PurchaseDetailEventData(
    Guid ProductId,
    decimal Quantity,
    decimal UnitCost);

public sealed record PurchaseCompletedDomainEvent(
    Guid PurchaseOrderId,
    TenantIdentifier TenantId,
    Guid ShiftId,
    decimal TotalAmount,
    IReadOnlyCollection<PurchaseDetailEventData> Details,
    DateTime OccurredAt) : INotification;