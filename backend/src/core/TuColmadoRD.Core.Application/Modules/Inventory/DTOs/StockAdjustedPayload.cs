namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Outbox payload for stock adjustment events.
/// </summary>
public sealed record StockAdjustedPayload(
    Guid ProductId,
    Guid TenantId,
    decimal Delta,
    decimal NewStock,
    string Reason,
    DateTime UpdatedAt);
