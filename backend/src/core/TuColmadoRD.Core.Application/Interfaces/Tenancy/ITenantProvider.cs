using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Application.Interfaces.Tenancy;

public interface ITenantProvider
{
    TenantIdentifier TenantId { get; }
    Guid TerminalId { get; }
    bool IsPaired { get; }
}
