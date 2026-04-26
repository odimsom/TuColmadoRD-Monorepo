using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Core.Domain.Entities.Fiscal
{
    public class FiscalReceipt : ITenantEntity
    {
    private FiscalReceipt() { }
        public Guid Id { get; private set; }
        public TenantIdentifier TenantId { get; private set; }
        public Guid SaleId { get; private set; }

        public string NCF { get; private set; }
        public Rnc? BuyerRnc { get; private set; }
        public Money TotalTaxed { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public string? TrackId { get; private set; }

        private FiscalReceipt(TenantIdentifier tenantId, Guid saleId, string ncf, Money totalTaxed, Rnc? rnc, string? trackId)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            SaleId = saleId;
            NCF = ncf;
            TotalTaxed = totalTaxed;
            BuyerRnc = rnc;
            IssuedAt = DateTime.UtcNow;
            TrackId = trackId;
        }

        public static FiscalReceipt Emit(TenantIdentifier tenantId, Guid saleId, string ncf, Money totalTaxed, Rnc? rnc = null, string? trackId = null)
        {
            return new FiscalReceipt(tenantId, saleId, ncf, totalTaxed, rnc, trackId);
        }
    }
}
