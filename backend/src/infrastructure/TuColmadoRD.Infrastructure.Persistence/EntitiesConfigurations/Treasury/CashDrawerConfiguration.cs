using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Treasury;

public class CashDrawerConfiguration : IEntityTypeConfiguration<CashDrawer>
{
    public void Configure(EntityTypeBuilder<CashDrawer> builder)
    {
        builder.ToTable("CashDrawers");
        builder.HasKey(cd => cd.Id);

        builder.OwnsOne(cd => cd.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(cd => cd.OpeningBalance, b => 
        {
            b.Property(m => m.Amount).HasColumnName("OpeningBalance").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(cd => cd.CurrentBalance, b => 
        {
            b.Property(m => m.Amount).HasColumnName("CurrentBalance").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Sales.Shift>()
            .WithMany()
            .HasForeignKey(cd => cd.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
