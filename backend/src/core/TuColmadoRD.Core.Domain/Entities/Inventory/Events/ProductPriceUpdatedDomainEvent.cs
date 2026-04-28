using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

/// <summary>
/// Raised when a product price is updated.
/// </summary>
public sealed record ProductPriceUpdatedDomainEvent(Guid ProductId, Guid TenantId, Money NewSalePrice, DateTime OccurredAt) : IDomainEvent;

