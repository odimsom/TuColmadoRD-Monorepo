using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

/// <summary>Response item for a presentation with critical stock level.</summary>
public sealed record LowStockItemDto(Guid PresentationId, Guid ProductId, string ProductName, string PresentationDisplayName, int StockQuantity, int Threshold);

/// <summary>Response wrapper for the low-stock endpoint.</summary>
public sealed record LowStockResponse(int Count, IReadOnlyList<LowStockItemDto> Items);

/// <summary>
/// Returns all active products whose stock is at or below <see cref="Threshold"/>.
/// Used by the dashboard "Stock Crítico" card (DASH-01).
/// </summary>
public sealed record GetLowStockQuery(int Threshold = 5)
    : IRequest<OperationResult<LowStockResponse, DomainError>>;
