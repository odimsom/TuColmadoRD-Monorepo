using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Customers;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Customers;

public class DebtTransactionConfiguration : IEntityTypeConfiguration<DebtTransaction>
{
    public void Configure(EntityTypeBuilder<DebtTransaction> builder)
    {
        builder.ToTable("DebtTransactions");
        builder.HasKey(dt => dt.Id);

        builder.OwnsOne(dt => dt.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(dt => dt.Amount, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Amount").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.Property(dt => dt.Concept).IsRequired().HasMaxLength(500);
        builder.Property(dt => dt.Type).IsRequired().HasConversion<string>();

        builder.HasOne<CustomerAccount>()
            .WithMany()
            .HasForeignKey(dt => dt.CustomerAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
