using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using SalesQuantity = TuColmadoRD.Core.Domain.Entities.Sales.Quantity;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
{
    public void Configure(EntityTypeBuilder<SaleDetail> builder)
    {
        builder.ToTable("SaleDetails");
        builder.HasKey(sd => sd.Id);

        builder.Property(sd => sd.Quantity)
            .HasConversion(v => v.Value, v => SalesQuantity.Of(v).Result)
            .HasColumnName("Quantity")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(sd => sd.UnitPrice)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(sd => sd.TaxAmount)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnName("TaxAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(sd => sd.SubTotal)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnName("SubTotal")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()
            .WithMany()
            .HasForeignKey(sd => sd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
