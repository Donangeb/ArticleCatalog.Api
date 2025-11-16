using ArticleCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArticleCatalog.Infrastructure.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> b)
    {
        b.ToTable("tags");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        b.HasIndex(x => x.Name).IsUnique();
    }
}
