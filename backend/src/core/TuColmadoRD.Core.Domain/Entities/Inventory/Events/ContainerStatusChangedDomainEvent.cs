using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

public sealed record ContainerStatusChangedDomainEvent(
    Guid ContainerId,
    Guid PresentationId,
    Guid TenantId,
    string NewStatus,
    DateTime OccurredAt) : IDomainEvent;
