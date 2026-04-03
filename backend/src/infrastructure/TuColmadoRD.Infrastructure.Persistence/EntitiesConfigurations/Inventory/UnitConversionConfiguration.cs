using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
    public void Configure(EntityTypeBuilder<UnitConversion> builder)
    {
        builder.ToTable("UnitConversions");
        builder.HasKey(uc => uc.Id);

        builder.OwnsOne(uc => uc.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(uc => uc.Factor).HasColumnType("decimal(18,6)").IsRequired();

        builder.HasOne(uc => uc.FromUnit)
            .WithMany()
            .HasForeignKey("FromUnitId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(uc => uc.ToUnit)
            .WithMany()
            .HasForeignKey("ToUnitId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(uc => uc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
