using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class UnitOfMeasureEntityConfiguration : IEntityTypeConfiguration<UnitOfMeasureEntity>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureEntity> builder)
    {
        builder.ToTable("UnitOfMeasures");
        builder.HasKey(uom => uom.Id);
        
        builder.Property(uom => uom.Id).HasConversion<string>();
        builder.Property(uom => uom.Name).IsRequired().HasMaxLength(50);
        builder.Property(uom => uom.Abbreviation).IsRequired().HasMaxLength(10);
    }
}
