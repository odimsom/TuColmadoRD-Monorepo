using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Logistics;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Logistics;

public class DeliveryPersonConfiguration : IEntityTypeConfiguration<DeliveryPerson>
{
    public void Configure(EntityTypeBuilder<DeliveryPerson> builder)
    {
        builder.ToTable("DeliveryPersons");
        builder.HasKey(d => d.Id);

        builder.OwnsOne(d => d.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);

        builder.OwnsOne(d => d.ContactPhone, b => 
        {
            b.Property(p => p.Value).HasColumnName("ContactPhone").HasMaxLength(20);
        });

        builder.Property(d => d.VehiclePlate).HasMaxLength(20);
    }
}
