using MediatR;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Commands;

/// <summary>
/// Request item for creating a sale.
/// </summary>
public sealed record SaleItemRequest(
    Guid ProductId,
    decimal Quantity);

/// <summary>
/// Request payment for creating a sale.
/// </summary>
public sealed record SalePaymentRequest(
    int PaymentMethodId,
    decimal Amount,
    string? Reference,
    Guid? CustomerId);

/// <summary>
/// Result item from sale creation.
/// </summary>
public sealed record SaleItemResult(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineItbis,
    decimal LineTotal);

/// <summary>
/// Result of creating a sale.
/// </summary>
public sealed record CreateSaleResult(
    Guid SaleId,
    string ReceiptNumber,
    /// <summary>
    /// NCF asignado por DGI. Null si el tenant no tiene secuencia fiscal configurada.
    /// </summary>
    string? NcfNumber,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    IReadOnlyList<SaleItemResult> Items);

/// <summary>
/// Command to create a new sale with items and payments.
/// </summary>
public sealed record CreateSaleCommand(
    IReadOnlyList<SaleItemRequest> Items,
    IReadOnlyList<SalePaymentRequest> Payments,
    string? Notes,
    /// <summary>
    /// RNC del comprador (9 dígitos). Si se provee, se emite B01 (crédito fiscal).
    /// Si es null, se emite B02 (consumidor final).
    /// </summary>
    string? BuyerRnc = null
) : IRequest<OperationResult<CreateSaleResult, DomainError>>, ICommandMarker;
