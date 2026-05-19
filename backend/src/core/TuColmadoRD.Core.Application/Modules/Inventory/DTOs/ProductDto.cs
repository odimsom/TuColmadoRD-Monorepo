namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Product read model projection.
/// </summary>
public sealed record ProductDto(
    Guid ProductId,
    string Name,
    Guid CategoryId,
    string CategoryName,
    decimal ItbisRate,
    bool IsActive,
    DateTime UpdatedAt);
