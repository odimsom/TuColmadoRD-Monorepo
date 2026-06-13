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
            // Lookup de secuencia activa: siempre se busca por tenant + prefijo
            b.HasIndex(t => t.Value).HasDatabaseName("IX_FiscalSequences_TenantId");
        });

        builder.Property(fs => fs.Prefix).IsRequired().HasMaxLength(5);

        // El UPDATE solo aplica si CurrentSequence no cambió desde la lectura:
        // dos ventas concurrentes ya no pueden emitir el mismo NCF (DGII exige
        // numeración única); la segunda falla con DbUpdateConcurrencyException
        // y el cliente reintenta.
        builder.Property(fs => fs.CurrentSequence).IsConcurrencyToken();
    }
}
