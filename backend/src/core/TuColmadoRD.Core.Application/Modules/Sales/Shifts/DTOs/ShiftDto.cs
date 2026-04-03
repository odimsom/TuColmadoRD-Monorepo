namespace TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

public sealed record ShiftDto(
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    string CashierName,
    string Status,
    decimal OpeningCashAmount,
    decimal? ClosingCashAmount,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal? ExpectedCashAmount,
    decimal? ActualCashAmount,
    decimal? CashDifference,
    string? Notes,
    int TotalSalesCount,
    decimal TotalSalesAmount);
