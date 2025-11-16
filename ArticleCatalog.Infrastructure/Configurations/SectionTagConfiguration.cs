using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations;

public class SectionTagConfiguration : IEntityTypeConfiguration<SectionTag>
{
    public void Configure(EntityTypeBuilder<SectionTag> b)
    {
        b.ToTable("section_tags");

        b.HasKey(x => new { x.SectionId, x.TagId });

        b.HasOne(x => x.Section)
            .WithMany(s => s.SectionTags)
            .HasForeignKey(x => x.SectionId);

        b.HasOne(x => x.Tag)
            .WithMany(t => t.SectionTags)
            .HasForeignKey(x => x.TagId);
    }
}
