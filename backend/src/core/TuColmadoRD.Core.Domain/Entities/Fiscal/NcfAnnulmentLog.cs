using System;
using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal;

public class NcfAnnulmentLog : ITenantEntity
{
    private NcfAnnulmentLog() { }

    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }
    public string NCF { get; private set; }
    public Guid SaleId { get; private set; }
    public string Reason { get; private set; }
    public DateTime AnnulledAt { get; private set; }

    private NcfAnnulmentLog(TenantIdentifier tenantId, string ncf, Guid saleId, string reason)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        NCF = ncf;
        SaleId = saleId;
        Reason = reason;
        AnnulledAt = DateTime.UtcNow;
    }

    public static NcfAnnulmentLog Create(TenantIdentifier tenantId, string ncf, Guid saleId, string reason)
    {
        return new NcfAnnulmentLog(tenantId, ncf, saleId, reason);
    }
}
