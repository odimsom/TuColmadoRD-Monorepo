namespace TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

public sealed record ShiftSummaryDto(
    Guid ShiftId,
    string CashierName,
    string Status,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    int TotalSalesCount,
    decimal TotalSalesAmount,
    decimal? CashDifference);
