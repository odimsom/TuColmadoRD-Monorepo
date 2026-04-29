using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Fiscal;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Fiscal;

public class NcfAnnulmentLogConfiguration : IEntityTypeConfiguration<NcfAnnulmentLog>
{
    public void Configure(EntityTypeBuilder<NcfAnnulmentLog> builder)
    {
        builder.ToTable("NcfAnnulmentLogs", "Fiscal");
        builder.HasKey(x => x.Id);
        
        builder.OwnsOne(x => x.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(x => x.NCF).HasMaxLength(13).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AnnulledAt).IsRequired();
    }
}
