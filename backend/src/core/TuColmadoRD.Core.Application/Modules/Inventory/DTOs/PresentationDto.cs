namespace TuColmadoRD.Core.Application.Inventory.DTOs;

public sealed record PresentationDto(
    Guid Id,
    Guid ProductId,
    string DisplayName,
    int PresentationTypeId,
    string PresentationTypeName,
    int SellModeId,
    string SellModeName,
    int MeasureUnit,
    string? Brand,
    decimal? NominalCapacity,
    decimal SalePrice,
    decimal CostPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
