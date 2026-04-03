using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal.Events;

public sealed record FiscalSequenceConsumedDomainEvent(
    Guid FiscalSequenceId,
    TenantIdentifier TenantId,
    string Ncf,
    DateTime OccurredAt);