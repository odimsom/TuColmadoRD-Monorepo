using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.SaleId).IsRequired();
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();

        builder.Property(i => i.QuantityValue).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(i => i.UnitPriceAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.CostPriceAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.ItbisRateValue).HasColumnType("decimal(9,6)").IsRequired();

        builder.Property(i => i.LineSubtotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.LineItbisAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.LineTotalAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasIndex(i => i.SaleId);
    }
}
