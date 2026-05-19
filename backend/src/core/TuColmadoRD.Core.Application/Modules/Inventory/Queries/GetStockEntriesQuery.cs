using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public sealed record StockEntryLineResultDto(
    Guid Id,
    Guid StockEntryId,
    Guid PresentationId,
    string PresentationDisplayName,
    int ContainerCount,
    int UnitsPerContainer,
    decimal NominalSizePerUnit,
    decimal CostPerUnit,
    decimal LineTotal);

public sealed record StockEntryResultDto(
    Guid Id,
    DateTime PurchasedAt,
    decimal TotalCost,
    string? SupplierName,
    string? Notes,
    Guid? FundTransactionId,
    IReadOnlyList<StockEntryLineResultDto> Lines);

public sealed record StockEntriesPagedResult(
    IReadOnlyList<StockEntryResultDto> Items,
    int TotalCount,
    int TotalPages);

public sealed record GetStockEntriesQuery(int Page = 1, int PageSize = 20)
    : IRequest<OperationResult<StockEntriesPagedResult, DomainError>>;
