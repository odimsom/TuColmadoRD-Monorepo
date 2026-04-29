using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.System;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.System;

public class TenantProfileConfiguration : IEntityTypeConfiguration<TenantProfile>
{
    public void Configure(EntityTypeBuilder<TenantProfile> builder)
    {
        builder.ToTable("TenantProfiles", "System");
        builder.HasKey(tp => tp.Id);

        builder.OwnsOne(tp => tp.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(tp => tp.BusinessName).IsRequired().HasMaxLength(200);
        builder.Property(tp => tp.BusinessAddress).IsRequired().HasMaxLength(500);
        builder.Property(tp => tp.Phone).HasMaxLength(20);
        builder.Property(tp => tp.Email).HasMaxLength(150);

        builder.OwnsOne(tp => tp.Rnc, b =>
        {
            b.Property(r => r.Value).HasColumnName("Rnc").HasMaxLength(15);
        });

        builder.Property(tp => tp.CreatedAt).IsRequired();
        builder.Property(tp => tp.UpdatedAt).IsRequired();
    }
}
