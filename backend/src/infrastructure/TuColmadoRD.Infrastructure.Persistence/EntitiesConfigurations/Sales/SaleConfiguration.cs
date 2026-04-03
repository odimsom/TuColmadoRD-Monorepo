using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShiftId).IsRequired();
        builder.Property(s => s.TerminalId).IsRequired();
        builder.Property(s => s.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.CashierName).HasMaxLength(120).IsRequired();
        builder.Property(s => s.StatusId).IsRequired();

        builder.Property(s => s.SubtotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.TotalItbisAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.TotalPaidAmount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(s => s.ChangeDueAmount).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(s => s.Notes).HasMaxLength(300);
        builder.Property(s => s.VoidReason).HasMaxLength(200);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.VoidedAt);

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Payments)
            .WithOne()
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
