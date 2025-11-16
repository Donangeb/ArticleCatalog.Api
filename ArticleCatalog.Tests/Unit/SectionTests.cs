using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Tests.Unit;

/// <summary>
/// Тесты для агрегата Section (Domain слой)
/// </summary>
public class SectionTests
{
    [Fact]
    public void Create_WithTags_ShouldCreateSection()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };

        // Act
        var section = Section.Create(tags);

        // Assert
        section.Should().NotBeNull();
        section.Id.Should().NotBeEmpty();
        section.TagCount.Should().Be(2);
        section.TagSetKey.Should().Be("tag1|tag2");
        section.Title.Should().Be("tag1, tag2");
        section.SectionTags.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithSingleTag_ShouldCreateSection()
    {
        // Arrange
        var tag = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tags = new[] { tag };

        // Act
        var section = Section.Create(tags);

        // Assert
        section.TagCount.Should().Be(1);
        section.TagSetKey.Should().Be("tag1");
        section.Title.Should().Be("tag1");
    }

    [Fact]
    public void Create_WithEmptyTags_ShouldCreateSection()
    {
        // Arrange
        var tags = Array.Empty<Tag>();

        // Act
        var section = Section.Create(tags);

        // Assert
        section.TagCount.Should().Be(0);
        section.TagSetKey.Should().BeEmpty();
        section.Title.Should().BeEmpty();
        section.SectionTags.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldAssociateTags()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };

        // Act
        var section = Section.Create(tags);

        // Assert
        section.SectionTags.Should().HaveCount(2);
        section.SectionTags.Should().Contain(st => st.TagId == tag1.Id);
        section.SectionTags.Should().Contain(st => st.TagId == tag2.Id);
        section.SectionTags.All(st => st.SectionId == section.Id).Should().BeTrue();
    }

    [Fact]
    public void MatchesTagSet_WithMatchingKey_ShouldReturnTrue()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var section = Section.Create(new[] { tag1, tag2 });
        var tagSetKey = TagSetKey.Create(new[] { "tag1", "tag2" });

        // Act
        var result = section.MatchesTagSet(tagSetKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void MatchesTagSet_WithDifferentKey_ShouldReturnFalse()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var section = Section.Create(new[] { tag1, tag2 });
        var tagSetKey = TagSetKey.Create(new[] { "tag1", "tag3" });

        // Act
        var result = section.MatchesTagSet(tagSetKey);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Create_WithLongTitle_ShouldTruncate()
    {
        // Arrange
        var longTagName = new string('a', 600);
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = longTagName };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };

        // Act
        var section = Section.Create(tags);

        // Assert
        section.Title.Length.Should().BeLessThanOrEqualTo(1024);
    }
}

