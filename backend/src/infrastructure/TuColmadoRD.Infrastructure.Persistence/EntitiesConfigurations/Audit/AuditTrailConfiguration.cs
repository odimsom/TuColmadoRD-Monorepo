using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Audit;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Audit;

public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("AuditTrails");
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(x => x.TableName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OldValues)
            .IsRequired();

        builder.Property(x => x.NewValues)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .IsRequired();
    }
}
