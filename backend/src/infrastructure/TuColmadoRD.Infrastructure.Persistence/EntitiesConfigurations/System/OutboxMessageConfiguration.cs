using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuColmadoRD.Core.Domain.Entities.System;

namespace TuColmadoRD.Infrastructure.Persistence.EntitiesConfigurations.System;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "System");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Payload).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.ProcessedAt).IsRequired(false);
        builder.Property(e => e.RetryCount).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.LastError).HasMaxLength(1000).IsRequired(false);

        builder.HasIndex(e => new { e.ProcessedAt, e.CreatedAt });
        builder.HasIndex(e => e.Type);
    }
}
