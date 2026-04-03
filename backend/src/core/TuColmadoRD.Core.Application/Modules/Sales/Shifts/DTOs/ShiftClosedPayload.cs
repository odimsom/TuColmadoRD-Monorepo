namespace TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

public sealed record ShiftClosedPayload(
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    string CashierName,
    decimal ActualCashAmount,
    decimal ExpectedCashAmount,
    decimal CashDifference,
    int TotalSalesCount,
    decimal TotalSalesAmount,
    DateTime ClosedAt);
