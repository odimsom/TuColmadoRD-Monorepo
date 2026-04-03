using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Customers;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Customers;

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        builder.ToTable("CustomerAccounts");
        builder.HasKey(ca => ca.Id);

        builder.OwnsOne(ca => ca.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(ca => ca.Balance, b => 
        {
            b.Property(m => m.Amount).HasColumnName("Balance").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.OwnsOne(ca => ca.CreditLimit, b => 
        {
            b.Property(m => m.Amount).HasColumnName("CreditLimit").HasColumnType("decimal(18,2)").IsRequired();
        });

        builder.HasMany("_transactions")
            .WithOne()
            .HasForeignKey("CustomerAccountId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation("_transactions").UsePropertyAccessMode(PropertyAccessMode.Field);

    }
}
