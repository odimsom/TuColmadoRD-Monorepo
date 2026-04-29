using MediatR;
using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales.Events;

/// <summary>
/// Record for a sale item line in domain events.
/// </summary>
public sealed record SaleItemEventLine(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal LineItbis);

/// <summary>
/// Record for a payment line in domain events.
/// </summary>
public sealed record SalePaymentEventLine(
    int PaymentMethodId,
    decimal Amount,
    string? Reference,
    Guid? CustomerId);

/// <summary>
/// Domain event raised when a sale is completed and finalized.
/// </summary>
public sealed record SaleCompletedDomainEvent(
    Guid SaleId,
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    string ReceiptNumber,
    string CashierName,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    IReadOnlyList<SaleItemEventLine> Items,
    IReadOnlyList<SalePaymentEventLine> Payments,
    DateTime OccurredAt) : IDomainEvent;

/// <summary>
/// Domain event raised when a sale is voided/cancelled.
/// </summary>
public sealed record SaleVoidedDomainEvent(
    Guid SaleId,
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    string ReceiptNumber,
    string VoidReason,
    DateTime OccurredAt) : IDomainEvent;
