namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Outbox payload for product deactivated events.
/// </summary>
public sealed record ProductDeactivatedPayload(
    Guid ProductId,
    Guid TenantId,
    DateTime UpdatedAt);
