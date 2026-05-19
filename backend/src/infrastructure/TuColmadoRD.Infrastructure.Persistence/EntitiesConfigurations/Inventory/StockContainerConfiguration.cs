using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.Inventory;

public class StockContainerConfiguration : IEntityTypeConfiguration<StockContainer>
{
    public void Configure(EntityTypeBuilder<StockContainer> builder)
    {
        builder.ToTable("StockContainers");
        builder.HasKey(c => c.Id);

        builder.OwnsOne(c => c.TenantId, b =>
        {
            b.Property(t => t.Value).HasColumnName("TenantId").IsRequired();
        });

        builder.Property(c => c.PresentationId).IsRequired();
        builder.Property(c => c.ContainerCode).IsRequired().HasMaxLength(20);
        builder.Property(c => c.NominalCapacity).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.ActualCapacity).HasColumnType("decimal(18,4)");
        builder.Property(c => c.CurrentRemaining).HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(c => c.IsActiveSource).IsRequired();
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.PurchasedAt).IsRequired();
        builder.Property(c => c.OpenedAt);
        builder.Property(c => c.EmptiedAt);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        builder.Property(c => c.Status)
            .HasConversion(v => v.Id, v => ContainerStatus.FromId(v).Result!)
            .IsRequired();

        builder.Ignore(c => c.DomainEvents);

        builder.HasIndex(c => c.PresentationId).HasDatabaseName("IX_StockContainers_PresentationId");
        builder.HasIndex(c => new { c.PresentationId, c.IsActiveSource }).HasDatabaseName("IX_StockContainers_PresentationId_IsActive");
    }
}
