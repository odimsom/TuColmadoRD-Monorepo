namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Outbox payload for product price updated events.
/// </summary>
public sealed record ProductPriceUpdatedPayload(
    Guid ProductId,
    Guid TenantId,
    decimal NewCostPrice,
    decimal NewSalePrice,
    DateTime UpdatedAt);
