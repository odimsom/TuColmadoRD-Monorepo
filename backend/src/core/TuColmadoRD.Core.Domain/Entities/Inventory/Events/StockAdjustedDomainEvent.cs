namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

/// <summary>
/// Raised when product stock is adjusted.
/// </summary>
public sealed record StockAdjustedDomainEvent(Guid ProductId, Guid TenantId, decimal Delta, decimal NewStock, DateTime OccurredAt);
