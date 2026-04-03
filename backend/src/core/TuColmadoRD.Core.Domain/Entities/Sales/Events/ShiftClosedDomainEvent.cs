namespace TuColmadoRD.Core.Domain.Entities.Sales.Events;

public sealed record ShiftClosedDomainEvent(
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    decimal ActualCash,
    decimal ExpectedCash,
    decimal CashDifference,
    int TotalSalesCount,
    decimal TotalSalesAmount,
    DateTime OccurredAt);
