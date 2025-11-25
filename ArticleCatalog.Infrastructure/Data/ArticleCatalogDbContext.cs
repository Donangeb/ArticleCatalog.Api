using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArticleCatalog.Infrastructure.Data
{
    public class ArticleCatalogDbContext : DbContext
    {
        public ArticleCatalogDbContext(DbContextOptions<ArticleCatalogDbContext> options)
            : base(options) { }

        public DbSet<Article> Article => Set<Article>();
        public DbSet<Tag> Tag => Set<Tag>();
        public DbSet<ArticleTag> ArticleTag => Set<ArticleTag>();
        public DbSet<Section> Section => Set<Section>();
        public DbSet<SectionTag> SectionTag => Set<SectionTag>();
        public DbSet<OutboxMessage> OutboxMessage => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.ApplyConfigurationsFromAssembly(typeof(ArticleCatalogDbContext).Assembly);
        }
    }

}

