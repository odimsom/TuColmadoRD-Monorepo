using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class ProductPresentationConfiguration : IEntityTypeConfiguration<ProductPresentation>
{
    public void Configure(EntityTypeBuilder<ProductPresentation> builder)
    {
        builder.ToTable("ProductPresentations");
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(p => p.ProductId).IsRequired();
        builder.Property(p => p.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Brand).HasMaxLength(80);
        builder.Property(p => p.NominalCapacity).HasColumnType("decimal(18,4)");
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();

        builder.Property(p => p.PresentationType)
            .HasConversion(v => v.Id, v => PresentationType.FromId(v).Result!)
            .IsRequired();

        builder.Property(p => p.SellMode)
            .HasConversion(v => v.Id, v => SellMode.FromId(v).Result!)
            .IsRequired();

        builder.Property(p => p.MeasureUnit)
            .HasConversion(v => (int)v, v => (UnitOfMeasure)v)
            .IsRequired();

        builder.Property(p => p.SalePrice)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.CostPrice)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Ignore(p => p.DomainEvents);

        builder.HasIndex(p => new { p.ProductId }).HasDatabaseName("IX_ProductPresentations_ProductId");
    }
}
