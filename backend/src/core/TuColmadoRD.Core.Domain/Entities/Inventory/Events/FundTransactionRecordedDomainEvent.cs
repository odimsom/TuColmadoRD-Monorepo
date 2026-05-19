using TuColmadoRD.Core.Domain.Base;

namespace TuColmadoRD.Core.Domain.Entities.Inventory.Events;

public sealed record FundTransactionRecordedDomainEvent(
    Guid TransactionId,
    Guid FundId,
    Guid TenantId,
    string TransactionType,
    decimal Amount,
    decimal BalanceAfter,
    DateTime OccurredAt) : IDomainEvent;
