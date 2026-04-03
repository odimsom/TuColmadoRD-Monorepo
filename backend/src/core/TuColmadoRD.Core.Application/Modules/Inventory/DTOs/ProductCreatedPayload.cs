namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Outbox payload for product created events.
/// </summary>
public sealed record ProductCreatedPayload(
    Guid ProductId,
    Guid TenantId,
    string Name,
    Guid CategoryId,
    decimal CostPrice,
    decimal SalePrice,
    decimal ItbisRate,
    int UnitType,
    DateTime CreatedAt);
