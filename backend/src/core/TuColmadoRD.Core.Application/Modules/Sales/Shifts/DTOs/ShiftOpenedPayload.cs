namespace TuColmadoRD.Core.Application.Sales.Shifts.DTOs;

public sealed record ShiftOpenedPayload(
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    string CashierName,
    decimal OpeningCashAmount,
    DateTime OpenedAt);
