using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Fiscal;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Fiscal;

public class FiscalReceiptConfiguration : IEntityTypeConfiguration<FiscalReceipt>
{
    public void Configure(EntityTypeBuilder<FiscalReceipt> builder)
    {
        builder.ToTable("FiscalReceipts");
        builder.HasKey(fr => fr.Id);

        builder.OwnsOne(fr => fr.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(fr => fr.NCF).IsRequired().HasMaxLength(20);

        builder.OwnsOne(fr => fr.TotalTaxed, b => 
        {
            b.Property(m => m.Amount).HasColumnName("TotalTaxed").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(fr => fr.BuyerRnc, b => 
        {
            b.Property(r => r.Value).HasColumnName("BuyerRnc").HasMaxLength(15);
        });
    }
}
