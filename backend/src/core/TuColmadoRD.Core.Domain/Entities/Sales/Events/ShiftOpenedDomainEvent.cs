using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Sales.Events;

public sealed record ShiftOpenedDomainEvent(
    Guid ShiftId,
    Guid TenantId,
    Guid TerminalId,
    decimal OpeningCash,
    string CashierName,
    DateTime OccurredAt) : IDomainEvent;
