using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Purchasing;

public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
    {
        builder.ToTable("PurchaseDetails");
        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.Quantity).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(pd => pd.UnitCost).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(pd => pd.SubTotal).HasColumnType("decimal(18,2)").IsRequired();

        builder.HasOne<TuColmadoRD.Core.Domain.Entities.Inventory.Product>()    
            .WithMany()
            .HasForeignKey(pd => pd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
