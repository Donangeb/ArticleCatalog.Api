using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations
{
    public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
    {
        public void Configure(EntityTypeBuilder<ArticleTag> b)
        {
            b.ToTable("article_tags");

            b.HasKey(x => new { x.ArticleId, x.TagId });

            b.Property(x => x.Position)
                .IsRequired();

            b.HasOne(x => x.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(x => x.ArticleId);

            b.HasOne(x => x.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(x => x.TagId);
        }
    }
}
