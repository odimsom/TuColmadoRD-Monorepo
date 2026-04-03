using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

/// <summary>
/// Raised when product price changes.
/// </summary>
public sealed record ProductPriceUpdatedDomainEvent(Guid ProductId, Guid TenantId, Money NewSalePrice, DateTime OccurredAt);
