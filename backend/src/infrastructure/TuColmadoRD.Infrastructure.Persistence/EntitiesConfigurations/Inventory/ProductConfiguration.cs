using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Property(p => p.CategoryId).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.OwnsOne(p => p.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
            b.HasIndex(t => t.Value).HasDatabaseName("IX_Products_TenantId");
        });

        builder.Property(p => p.ItbisRate)
            .HasConversion(v => v.Rate, v => TaxRate.Create(v).Result)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Presentations);
    }
}
