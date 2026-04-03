using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
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
        builder.Property(p => p.StockQuantity).HasColumnType("decimal(18,4)").IsRequired();

        builder.OwnsOne(p => p.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(p => p.CostPrice)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.SalePrice)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.ItbisRate)
            .HasConversion(v => v.Rate, v => TaxRate.Create(v).Result)
            .HasColumnType("decimal(5,4)")
            .IsRequired();

        builder.Property(p => p.UnitType)
            .HasConversion(v => v.Id, v => UnitType.FromId(v).Result)
            .IsRequired();

        builder.Ignore(p => p.DomainEvents);
    }
}
