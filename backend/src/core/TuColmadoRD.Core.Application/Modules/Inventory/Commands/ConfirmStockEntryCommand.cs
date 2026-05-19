using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public record StockEntryLineDto(
    Guid PresentationId,
    int ContainerCount,
    int UnitsPerContainer,
    decimal NominalSizePerUnit,
    decimal CostPerUnit);

public record ConfirmStockEntryCommand(
    DateTime PurchasedAt,
    string? SupplierName,
    string? Notes,
    Guid? FundId,
    string? FundExpenseJustification,
    IReadOnlyList<StockEntryLineDto> Lines
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;
