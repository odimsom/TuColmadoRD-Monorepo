using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Sales;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Sales;

public sealed class SalePaymentConfiguration : IEntityTypeConfiguration<SalePayment>
{
    public void Configure(EntityTypeBuilder<SalePayment> builder)
    {
        builder.ToTable("SalePayments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.SaleId).IsRequired();
        builder.Property(p => p.PaymentMethodId).IsRequired();
        builder.Property(p => p.AmountValue).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.CustomerId);
        builder.Property(p => p.ReceivedAt).IsRequired();

        builder.HasIndex(p => p.SaleId);
        builder.HasIndex(p => p.CustomerId);
    }
}
