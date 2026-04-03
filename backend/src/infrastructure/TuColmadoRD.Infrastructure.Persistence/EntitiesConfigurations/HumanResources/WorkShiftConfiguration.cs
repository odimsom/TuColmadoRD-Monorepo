using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.HumanResources;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.HumanResources;

public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
        builder.ToTable("WorkShifts");
        builder.HasKey(e => e.Id);

        builder.OwnsOne(e => e.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(e => e.Name).IsRequired().HasMaxLength(150);
    }
}
