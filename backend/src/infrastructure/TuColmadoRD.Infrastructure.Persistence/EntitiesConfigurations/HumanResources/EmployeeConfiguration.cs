using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.HumanResources;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.HumanResources;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);

        builder.OwnsOne(e => e.IdCard, b => 
        {
            b.Property(c => c.Value).HasColumnName("IdCard").HasMaxLength(20);
        });

        builder.OwnsOne(e => e.Phone, b => 
        {
            b.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        builder.Property(e => e.Role).IsRequired().HasConversion<string>();
    }
}
