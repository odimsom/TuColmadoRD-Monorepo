using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Treasury;

public class CashBoxConfiguration : IEntityTypeConfiguration<CashBox>
{
    public void Configure(EntityTypeBuilder<CashBox> builder)
    {
        builder.ToTable("CashBoxes");
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);

        builder.OwnsOne(c => c.Balance, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Balance").HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}
