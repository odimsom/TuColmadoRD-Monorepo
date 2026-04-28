namespace TuColmadoRD.Presentation.API.Endpoints.Sales;

public sealed record SaleItemRequest(Guid ProductId, decimal Quantity);

public sealed record SalePaymentRequest(int PaymentMethodId, decimal Amount, string? Reference, Guid? CustomerId);

public sealed record CreateSaleRequest(
    IReadOnlyList<SaleItemRequest> Items,
    IReadOnlyList<SalePaymentRequest> Payments,
    string? Notes,
    /// <summary>RNC del comprador. Si se envía, se emite B01 (crédito fiscal); de lo contrario B02 (consumidor final).</summary>
    string? BuyerRnc = null);

public sealed record CreateSaleResponse(
    Guid SaleId,
    string ReceiptNumber,
    /// <summary>NCF asignado (ej. B0200000001). Null si el tenant no tiene secuencia fiscal activa.</summary>
    string? NcfNumber,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    IReadOnlyList<CreateSaleLineResponse> Items);

public sealed record CreateSaleLineResponse(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineItbis,
    decimal LineTotal);

public sealed record VoidSaleRequest(string VoidReason);

public sealed record SaleItemResponse(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal CostPrice,
    decimal ItbisRate,
    decimal Subtotal,
    decimal Itbis,
    decimal Total);

public sealed record SalePaymentResponse(
    int PaymentMethodId,
    decimal Amount,
    string? Reference,
    Guid? CustomerId,
    DateTime ReceivedAt);

public sealed record SaleDetailResponse(
    Guid SaleId,
    Guid ShiftId,
    Guid TerminalId,
    string ReceiptNumber,
    string CashierName,
    int StatusId,
    decimal Subtotal,
    decimal TotalItbis,
    decimal Total,
    decimal TotalPaid,
    decimal ChangeDue,
    string? Notes,
    DateTime CreatedAt,
    DateTime? VoidedAt,
    string? VoidReason,
    IReadOnlyList<SaleItemResponse> Items,
    IReadOnlyList<SalePaymentResponse> Payments);

public sealed record SaleSummaryResponse(
    Guid SaleId,
    string ReceiptNumber,
    int StatusId,
    decimal Total,
    decimal TotalPaid,
    DateTime CreatedAt,
    int ItemCount);

public sealed record PagedSalesResponse(
    IReadOnlyList<SaleSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    decimal TotalRevenue);
