using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Fiscal;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Fiscal;

public class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.ToTable("Taxes");
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Type).IsRequired().HasConversion<string>();

        builder.Property(t => t.Rate)
            .HasConversion(v => v.Rate, v => TaxRate.Create(v).Result)
            .HasColumnName("RatePercentage")
            .HasColumnType("decimal(5,4)")
            .IsRequired();
    }
}
