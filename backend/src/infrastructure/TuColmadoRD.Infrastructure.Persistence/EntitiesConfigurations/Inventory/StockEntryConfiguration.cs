using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.ToTable("StockEntries");
        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(e => e.PurchasedAt).IsRequired();
        builder.Property(e => e.SupplierName).HasMaxLength(120);
        builder.Property(e => e.Notes).HasMaxLength(500);
        builder.Property(e => e.FundTransactionId);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.Property(e => e.TotalCost)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.StockEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}

public class StockEntryLineConfiguration : IEntityTypeConfiguration<StockEntryLine>
{
    public void Configure(EntityTypeBuilder<StockEntryLine> builder)
    {
        builder.ToTable("StockEntryLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.StockEntryId).IsRequired();
        builder.Property(l => l.PresentationId).IsRequired();
        builder.Property(l => l.ContainerCount).IsRequired();
        builder.Property(l => l.UnitsPerContainer).IsRequired();
        builder.Property(l => l.NominalSizePerUnit).HasColumnType("decimal(18,4)").IsRequired();

        builder.Property(l => l.CostPerUnit)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Ignore(l => l.TotalUnits);
        builder.Ignore(l => l.TotalNominalWeight);

        builder.HasIndex(l => l.StockEntryId).HasDatabaseName("IX_StockEntryLines_StockEntryId");
    }
}
