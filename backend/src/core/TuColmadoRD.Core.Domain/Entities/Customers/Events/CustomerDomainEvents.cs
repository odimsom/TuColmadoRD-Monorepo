using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Customers.Events
{
    public sealed record CustomerCreatedDomainEvent(
        Guid CustomerId,
        TenantIdentifier TenantId,
        string FullName,
        DateTime OccurredOn
    );

    public sealed record ChargeRegisteredDomainEvent(
        Guid AccountId,
        TenantIdentifier TenantId,
        Guid CustomerId,
        Money Amount,
        Money NewBalance,
        Guid DebtTransactionId,
        DateTime OccurredOn
    );

    public sealed record PaymentRegisteredDomainEvent(
        Guid AccountId,
        TenantIdentifier TenantId,
        Guid CustomerId,
        Money Amount,
        Money NewBalance,
        Guid DebtTransactionId,
        DateTime OccurredOn
    );
}