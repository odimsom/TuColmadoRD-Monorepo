using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

/// <summary>
/// Raised when a product is created.
/// </summary>
public sealed record ProductCreatedDomainEvent(Guid ProductId, Guid TenantId, string Name, DateTime OccurredAt);
