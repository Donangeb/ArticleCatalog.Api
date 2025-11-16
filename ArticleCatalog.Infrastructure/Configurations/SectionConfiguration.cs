using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> b)
    {
        b.ToTable("sections");

        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(1024)
            .IsRequired();

        b.Property(x => x.TagSetKey)
            .HasMaxLength(2048)
            .IsRequired();

        b.Property(x => x.TagCount)
            .IsRequired();

        b.HasIndex(x => x.TagSetKey).IsUnique();

        // Настройка приватной коллекции для EF Core
        b.Metadata.FindNavigation(nameof(Section.SectionTags))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
