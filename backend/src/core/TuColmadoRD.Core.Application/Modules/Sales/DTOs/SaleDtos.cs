namespace TuColmadoRD.Core.Application.Sales.DTOs;

/// <summary>
/// Single item line in a sale detail.
/// </summary>
public sealed record SaleItemDto(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal ItbisRate,
    decimal LineSubtotal,
    decimal LineItbis,
    decimal LineTotal);

/// <summary>
/// Single payment line in a sale detail.
/// </summary>
public sealed record SalePaymentDto(
    int PaymentMethodId,
    string PaymentMethodName,
    decimal Amount,
    string? Reference,
    DateTime ReceivedAt);

/// <summary>
/// Full sale detail with all items and payments.
/// </summary>
public sealed record SaleDetailDto(
    Guid SaleId,
    Guid ShiftId,
    string ReceiptNumber,
    string CashierName,
    string Status,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    string? Notes,
    DateTime CreatedAt,
    DateTime? VoidedAt,
    string? VoidReason,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<SalePaymentDto> Payments);

/// <summary>
/// Summary of sale for paged listing.
/// </summary>
public sealed record SaleSummaryDto(
    Guid SaleId,
    string ReceiptNumber,
    string CashierName,
    string Status,
    decimal Total,
    decimal TotalPaid,
    DateTime CreatedAt,
    int ItemCount);

/// <summary>
/// Line item for receipt printing.
/// </summary>
public sealed record ReceiptLineDto(
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal ItbisRate,
    decimal LineTotal);

/// <summary>
/// Receipt format for printing/frontend display.
/// </summary>
public sealed record ReceiptDto(
    string ReceiptNumber,
    string CashierName,
    DateTime CreatedAt,
    Guid TenantId,
    IReadOnlyList<ReceiptLineDto> Items,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    IReadOnlyList<SalePaymentDto> Payments);

/// <summary>
/// Paged result container for sales.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
