using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

public sealed record PresentationCreatedDomainEvent(
    Guid PresentationId,
    Guid ProductId,
    Guid TenantId,
    string DisplayName,
    DateTime OccurredAt) : IDomainEvent;
