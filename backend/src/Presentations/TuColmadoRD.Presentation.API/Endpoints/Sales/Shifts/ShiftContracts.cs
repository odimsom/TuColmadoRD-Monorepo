using TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

namespace TuColmadoRD.Presentation.API.Endpoints.Sales.Shifts;

public sealed record OpenShiftRequest(decimal OpeningCashAmount, string CashierName);

public sealed record OpenShiftResponse(Guid ShiftId);

public sealed record CloseShiftRequest(decimal ActualCashAmount, string? Notes);

public sealed record CloseShiftResponse(
    Guid ShiftId,
    int TotalSalesCount,
    decimal TotalSalesAmount,
    decimal ExpectedCashAmount,
    decimal ActualCashAmount,
    decimal CashDifference,
    DateTime ClosedAt);

public sealed record PagedShiftResponse(
    IReadOnlyList<ShiftSummaryDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
