namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

/// <summary>
/// Raised when a product is deactivated.
/// </summary>
public sealed record ProductDeactivatedDomainEvent(Guid ProductId, Guid TenantId, DateTime OccurredAt);
