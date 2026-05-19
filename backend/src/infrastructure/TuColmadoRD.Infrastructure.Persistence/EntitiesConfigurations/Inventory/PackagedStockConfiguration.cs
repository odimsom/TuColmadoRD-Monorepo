using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class PackagedStockConfiguration : IEntityTypeConfiguration<PackagedStock>
{
    public void Configure(EntityTypeBuilder<PackagedStock> builder)
    {
        builder.ToTable("PackagedStock");
        builder.HasKey(p => p.Id);

        builder.OwnsOne(p => p.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(p => p.PresentationId).IsRequired();
        builder.Property(p => p.Quantity).IsRequired();
        builder.Property(p => p.LastUpdatedAt).IsRequired();

        builder.HasIndex(p => p.PresentationId).IsUnique().HasDatabaseName("IX_PackagedStock_PresentationId");
    }
}
