namespace TuColmadoRD.Core.Application.Inventory.DTOs;

public sealed record PresentationDto(
    Guid Id,
    Guid ProductId,
    string DisplayName,
    int PresentationType,
    string PresentationTypeName,
    int SellMode,
    string SellModeName,
    int MeasureUnit,
    string? Brand,
    decimal? NominalCapacity,
    decimal SalePrice,
    decimal CostPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int PackagedStockQuantity,
    int OpenContainersCount);
