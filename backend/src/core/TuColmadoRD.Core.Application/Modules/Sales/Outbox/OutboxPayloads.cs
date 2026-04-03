namespace TuColmadoRD.Core.Application.Sales.Outbox;

/// <summary>
/// Line item in SaleCreatedPayload.
/// </summary>
public sealed record SaleItemPayloadLine(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal CostPrice,
    decimal ItbisRate,
    decimal LineSubtotal,
    decimal LineItbis,
    decimal LineTotal);

/// <summary>
/// Payment line in SaleCreatedPayload.
/// </summary>
public sealed record SalePaymentPayloadLine(
    int PaymentMethodId,
    decimal Amount,
    string? Reference,
    Guid? CustomerId);

/// <summary>
/// Outbox payload when sale is created.
/// Contains full invoice details for cloud sync and fiscal receipt generation.
/// </summary>
public sealed record SaleCreatedPayload(
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
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<SaleItemPayloadLine> Items,
    IReadOnlyList<SalePaymentPayloadLine> Payments);

/// <summary>
/// Outbox payload when sale is voided.
/// </summary>
public sealed record SaleVoidedPayload(
    Guid SaleId,
    Guid TenantId,
    Guid TerminalId,
    string ReceiptNumber,
    string VoidReason,
    DateTime VoidedAt);
