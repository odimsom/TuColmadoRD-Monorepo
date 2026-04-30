using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Logistics;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Logistics;

public class DeliveryOrderConfiguration : IEntityTypeConfiguration<DeliveryOrder>
{
    public void Configure(EntityTypeBuilder<DeliveryOrder> builder)
    {
        builder.ToTable("DeliveryOrders");
        builder.HasKey(d => d.Id);

        builder.OwnsOne(d => d.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(d => d.Destination, b => 
        {
            b.Property(a => a.Province).HasColumnName("Destination_Province").HasMaxLength(100);
            b.Property(a => a.Sector).HasColumnName("Destination_Sector").HasMaxLength(100);
            b.Property(a => a.Street).HasColumnName("Destination_Street").HasMaxLength(100);
            b.Property(a => a.HouseNumber).HasColumnName("Destination_HouseNumber").HasMaxLength(20);
            b.Property(a => a.Reference).HasColumnName("Destination_Reference").HasMaxLength(200);
            b.Property(a => a.Latitude).HasColumnName("Destination_Latitude");
            b.Property(a => a.Longitude).HasColumnName("Destination_Longitude");
        });

        builder.Property(d => d.ConfirmationCode).IsRequired().HasMaxLength(6);

        builder.Property(d => d.Status).IsRequired().HasConversion<string>();

        builder.HasOne<DeliveryPerson>()
            .WithMany()
            .HasForeignKey(d => d.DeliveryPersonId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
