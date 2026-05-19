using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record CatalogPresentationDto(
    Guid Id,
    Guid ProductId,
    string DisplayName,
    int PresentationType,
    string PresentationTypeName,
    int SellMode,
    string SellModeName,
    string? Brand,
    decimal? NominalCapacity,
    int MeasureUnit,
    string MeasureUnitName,
    decimal SalePrice,
    decimal CostPrice,
    bool IsActive,
    int StockQuantity,
    int OpenContainersCount,
    int PackagedStockQuantity);

public sealed record CatalogItemDto(
    Guid ProductId,
    string Name,
    Guid CategoryId,
    string CategoryName,
    decimal ItbisRate,
    bool IsActive,
    IReadOnlyList<CatalogPresentationDto> Presentations);

public sealed record GetCatalogQuery() : IRequest<OperationResult<IReadOnlyList<CatalogItemDto>, DomainError>>;
