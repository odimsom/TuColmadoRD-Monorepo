using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Customers;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(c => c.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.OwnsOne(c => c.DocumentId, b => 
        {
            b.Property(d => d.Value).HasColumnName("DocumentId").IsRequired().HasMaxLength(20);
        });

        builder.OwnsOne(c => c.ContactPhone, b => 
        {
            b.Property(p => p.Value).HasColumnName("ContactPhone").HasMaxLength(20);
        });

        builder.OwnsOne(c => c.HomeAddress, b =>
        {
            b.Property(a => a.Province).HasColumnName("Address_Province").HasMaxLength(100);
            b.Property(a => a.Sector).HasColumnName("Address_Sector").HasMaxLength(100);
            b.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(100);
            b.Property(a => a.HouseNumber).HasColumnName("Address_HouseNumber").HasMaxLength(20);
            b.Property(a => a.Reference).HasColumnName("Address_Reference").HasMaxLength(200);
            b.Property(a => a.Latitude).HasColumnName("Address_Latitude");
            b.Property(a => a.Longitude).HasColumnName("Address_Longitude");
        });

    }
}
