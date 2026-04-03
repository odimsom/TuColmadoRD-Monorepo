namespace TuColmadoRD.Core.Application.Inventory.DTOs;

/// <summary>
/// Product read model projection.
/// </summary>
public sealed record ProductDto(
    Guid ProductId,
    string Name,
    Guid CategoryId,
    string CategoryName,
    decimal CostPrice,
    decimal SalePrice,
    decimal ItbisRate,
    int UnitTypeId,
    string UnitTypeName,
    decimal StockQuantity,
    bool IsActive,
    DateTime UpdatedAt);
