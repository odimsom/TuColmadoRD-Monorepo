using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Treasury;

public class PettyCashConfiguration : IEntityTypeConfiguration<PettyCash>
{
    public void Configure(EntityTypeBuilder<PettyCash> builder)
    {
        builder.ToTable("PettyCashes");
        builder.HasKey(pc => pc.Id);

        builder.OwnsOne(pc => pc.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(pc => pc.Name).IsRequired().HasMaxLength(150);

        builder.OwnsOne(pc => pc.Balance, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Balance").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(pc => pc.MaxLimit, b => 
        {
            b.Property(m => m.Amount).HasColumnName("MaxLimit").HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}
