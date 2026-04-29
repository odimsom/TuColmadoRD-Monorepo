using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Interfaces.Repositories.Fiscal;

public interface IFiscalSequenceRepository : IGenericRepository<FiscalSequence>
{
    /// <summary>
    /// Returns the active (non-expired, non-exhausted) fiscal sequence for
    /// the given tenant and NCF prefix (e.g. "B01", "B02").
    /// Returns null when no active sequence exists — caller must surface a
    /// user-friendly error so the operator can register a new NCF range.
    /// </summary>
    Task<FiscalSequence?> GetActiveByPrefixAsync(
        TenantIdentifier tenantId,
        string prefix,
        CancellationToken cancellationToken = default);
}
