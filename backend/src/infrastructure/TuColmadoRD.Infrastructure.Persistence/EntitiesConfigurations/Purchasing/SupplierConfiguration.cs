using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Purchasing;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.OwnsOne(s => s.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();    
        });

        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);   

        builder.OwnsOne(s => s.Rnc, b =>
        {
            b.Property(r => r.Value).HasColumnName("Rnc").HasMaxLength(15);   
        });

        builder.Property(s => s.Type).IsRequired().HasConversion<string>();     
    }
}
