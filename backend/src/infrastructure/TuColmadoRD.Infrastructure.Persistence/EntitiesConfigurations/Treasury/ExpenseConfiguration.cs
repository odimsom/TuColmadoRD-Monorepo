using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Treasury;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Treasury;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);

        builder.OwnsOne(e => e.Amount, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Property(e => e.Category).IsRequired().HasConversion<string>();
        builder.Property(e => e.ReferenceNumber).HasMaxLength(50);

        builder.HasOne<CashBox>()
            .WithMany()
            .HasForeignKey(e => e.CashBoxId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
