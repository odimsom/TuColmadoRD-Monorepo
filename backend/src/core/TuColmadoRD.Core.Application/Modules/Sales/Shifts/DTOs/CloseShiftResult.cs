namespace TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

public sealed record CloseShiftResult(
    Guid ShiftId,
    int TotalSalesCount,
    decimal TotalSalesAmount,
    decimal ExpectedCashAmount,
    decimal ActualCashAmount,
    decimal CashDifference,
    DateTime ClosedAt);
