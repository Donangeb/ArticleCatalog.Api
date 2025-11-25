using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("outbox_messages");
        b.HasKey(x => x.Id);

        b.Property(x => x.EventType)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.EventData)
            .IsRequired();

        b.Property(x => x.OccurredOn)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.ProcessedAt)
            .IsRequired(false);

        b.Property(x => x.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        // Индекс для оптимизации выборки необработанных сообщений
        b.HasIndex(x => new { x.IsProcessed, x.CreatedAt });
    }
}

