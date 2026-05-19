namespace TuColmadoRD.Core.Application.Inventory.DTOs;

public sealed record ContainerDto(
    Guid Id,
    Guid PresentationId,
    string ContainerCode,
    decimal NominalCapacity,
    decimal? ActualCapacity,
    decimal CurrentRemaining,
    string Status,
    bool IsActiveSource,
    string Notes,
    DateTime PurchasedAt,
    DateTime? OpenedAt,
    DateTime? EmptiedAt,
    DateTime CreatedAt);
