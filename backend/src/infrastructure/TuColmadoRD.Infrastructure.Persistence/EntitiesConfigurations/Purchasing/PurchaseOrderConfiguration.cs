using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Purchasing;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.OwnsOne(po => po.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();    
        });

        builder.Property(po => po.Status).IsRequired().HasConversion<string>(); 

        builder.Property(po => po.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(po => po.SupplierNcf).HasMaxLength(20);

        builder.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(po => po.Details)
            .WithOne()
            .HasForeignKey(d => d.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(PurchaseOrder.Details))!
               .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
