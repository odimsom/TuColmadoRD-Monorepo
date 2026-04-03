using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Fiscal.Events;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal;

public class FiscalSequence : ITenantEntity
{
    private readonly List<object> _domainEvents = [];
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public string Prefix { get; private set; }
    public int CurrentSequence { get; private set; }
    public int EndSequence { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public bool IsActive { get; private set; }

    private FiscalSequence()
    {
        Prefix = string.Empty;
        TenantId = null!;
    }

    private FiscalSequence(TenantIdentifier tenantId, string prefix, int start, int end, DateTime validUntil)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Prefix = prefix;
        CurrentSequence = start;
        EndSequence = end;
        ValidUntil = validUntil;
        IsActive = true;
    }

    public static OperationResult<FiscalSequence, DomainError> Create(
        TenantIdentifier tenantId, string prefix, int start, int end, DateTime validUntil)
    {
        if (end <= start)
            return OperationResult<FiscalSequence, DomainError>.Bad(DomainError.Validation("FiscalSequence.InvalidRange", "El número final debe ser mayor al inicial."));

        if (validUntil.ToUniversalTime() <= DateTime.UtcNow)
            return OperationResult<FiscalSequence, DomainError>.Bad(DomainError.Validation("FiscalSequence.Expired", "La secuencia ya está vencida."));

        return OperationResult<FiscalSequence, DomainError>.Good(new FiscalSequence(tenantId, prefix, start, end, validUntil.ToUniversalTime()));
    }

    public OperationResult<string, DomainError> GetNextNcf()
    {
        if (!IsActive)
            return OperationResult<string, DomainError>.Bad(DomainError.Validation("FiscalSequence.Inactive", "Secuencia fiscal inactiva."));

        if (DateTime.UtcNow > ValidUntil)
            return OperationResult<string, DomainError>.Bad(DomainError.Validation("FiscalSequence.Expired", "Secuencia fiscal vencida."));

        if (CurrentSequence > EndSequence)
            return OperationResult<string, DomainError>.Bad(DomainError.Validation("FiscalSequence.Exhausted", "Secuencia fiscal agotada."));

        string fullNumber = $"{Prefix}{CurrentSequence:D8}";
        CurrentSequence++;

        _domainEvents.Add(new FiscalSequenceConsumedDomainEvent(
            Id,
            TenantId,
            fullNumber,
            DateTime.UtcNow));

        return OperationResult<string, DomainError>.Good(fullNumber);
    }
}
