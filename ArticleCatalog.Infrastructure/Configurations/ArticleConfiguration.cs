using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> b)
    {
        b.ToTable("articles");
        b.HasKey(x => x.Id);

        b.Property(x => x.Title)
            .HasMaxLength(256)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.UpdatedAt)
            .IsRequired(false);

        b.Property(x => x.TagSetKey)
            .HasMaxLength(2048)
            .IsRequired();

        // Индекс для оптимизации поиска по TagSetKey
        b.HasIndex(x => x.TagSetKey);

        // Настройка приватной коллекции для EF Core
        b.Metadata.FindNavigation(nameof(Article.ArticleTags))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
