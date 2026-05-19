using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using TuColmadoRD.Core.Domain.ValueObjects;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class MonetaryFundConfiguration : IEntityTypeConfiguration<MonetaryFund>
{
    public void Configure(EntityTypeBuilder<MonetaryFund> builder)
    {
        builder.ToTable("MonetaryFunds");
        builder.HasKey(f => f.Id);

        builder.OwnsOne(f => f.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(f => f.Name).IsRequired().HasMaxLength(80);
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.Property(f => f.UpdatedAt).IsRequired();

        builder.Property(f => f.CurrentBalance)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasMany(f => f.Transactions)
            .WithOne()
            .HasForeignKey(t => t.FundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(f => f.DomainEvents);
    }
}

public class FundTransactionConfiguration : IEntityTypeConfiguration<FundTransaction>
{
    public void Configure(EntityTypeBuilder<FundTransaction> builder)
    {
        builder.ToTable("FundTransactions");
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.TenantId, b =>
        {
            b.Property(ten => ten.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(t => t.FundId).IsRequired();
        builder.Property(t => t.Description).IsRequired().HasMaxLength(200);
        builder.Property(t => t.JustificationNote).HasMaxLength(500);
        builder.Property(t => t.ReferenceId);
        builder.Property(t => t.OccurredAt).IsRequired();

        builder.Property(t => t.Type)
            .HasConversion(v => v.Id, v => FundTransactionType.FromId(v).Result!)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasConversion(
                v => v != null ? (int?)v.Id : null,
                v => v.HasValue ? FundExpenseCategory.FromId(v.Value).Result : null);

        builder.Property(t => t.Amount)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasConversion(v => v.Amount, v => Money.FromDecimal(v).Result!)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasIndex(t => t.FundId).HasDatabaseName("IX_FundTransactions_FundId");
    }
}
