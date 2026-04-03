using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Enums.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TerminalId).IsRequired();
        builder.Property(s => s.CashierName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.OpenedAt).IsRequired();
        builder.Property(s => s.ClosedAt).IsRequired(false);
        builder.Property(s => s.Notes).HasMaxLength(500).IsRequired(false);
        builder.Property(s => s.TotalSalesCount).IsRequired();

        builder.OwnsOne(s => s.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(s => s.Status)
            .HasConversion(v => v.Id, v => ShiftStatus.FromId(v).Result)
            .IsRequired();

        builder.Property(s => s.OpeningCashAmount)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.TotalSalesAmount)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.TotalExpenses)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.TotalCashSales)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.TotalAccountPayments)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.TotalCashIn)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.TotalCardIn)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.TotalTransferIn)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result)     
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        builder.Property(s => s.ClosingCashAmount)
            .HasConversion(v => v == null ? (decimal?)null : v.Amount, v => v == null ? null : Money.FromDecimal(v.Value).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(s => s.ExpectedCashAmount)
            .HasConversion(v => v == null ? (decimal?)null : v.Amount, v => v == null ? null : Money.FromDecimal(v.Value).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(s => s.ActualCashAmount)
            .HasConversion(v => v == null ? (decimal?)null : v.Amount, v => v == null ? null : Money.FromDecimal(v.Value).Result)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(s => s.CashDifferenceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Ignore(s => s.DomainEvents);

        builder.HasIndex(nameof(Shift.TerminalId), nameof(Shift.Status));
    }
}
