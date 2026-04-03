using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Fiscal;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Fiscal;

public class FiscalSequenceConfiguration : IEntityTypeConfiguration<FiscalSequence>
{
    public void Configure(EntityTypeBuilder<FiscalSequence> builder)
    {
        builder.ToTable("FiscalSequences");
        builder.HasKey(fs => fs.Id);

        builder.OwnsOne(fs => fs.TenantId, b => 
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });


        builder.Property(fs => fs.Prefix).IsRequired().HasMaxLength(5);
    }
}
