using ArticleCatalog.Domain.DomainServices;
using ArticleCatalog.Domain.Entities;
using FluentAssertions;

namespace ArticleCatalog.Domain.Tests.Unit;

/// <summary>
/// Тесты для доменного сервиса SectionGenerator (Domain слой)
/// Примечание: Тесты для Application.SectionService находятся в ArticleCatalog.Application.Tests.SectionServiceTests
/// </summary>
public class SectionServiceTests
{
    private readonly SectionGenerator _sectionGenerator;

    public SectionServiceTests()
    {
        _sectionGenerator = new SectionGenerator();
    }

    [Fact]
    public void CreateSectionForTags_WithTags_ShouldCreateSection()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };

        // Act
        var section = _sectionGenerator.CreateSectionForTags(tags);

        // Assert
        section.Should().NotBeNull();
        section.Id.Should().NotBeEmpty();
        section.TagCount.Should().Be(2);
        section.TagSetKey.Should().Be("tag1|tag2");
        section.Title.Should().Be("tag1, tag2");
        section.SectionTags.Should().HaveCount(2);
    }

    [Fact]
    public void CreateSectionForTags_WithSingleTag_ShouldCreateSection()
    {
        // Arrange
        var tag = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tags = new[] { tag };

        // Act
        var section = _sectionGenerator.CreateSectionForTags(tags);

        // Assert
        section.Should().NotBeNull();
        section.TagCount.Should().Be(1);
        section.TagSetKey.Should().Be("tag1");
        section.Title.Should().Be("tag1");
    }

    [Fact]
    public void CreateSectionForTags_WithEmptyTags_ShouldCreateEmptySection()
    {
        // Arrange
        var tags = Array.Empty<Tag>();

        // Act
        var section = _sectionGenerator.CreateSectionForTags(tags);

        // Assert
        section.Should().NotBeNull();
        section.TagCount.Should().Be(0);
        section.TagSetKey.Should().BeEmpty();
        section.Title.Should().BeEmpty();
        section.SectionTags.Should().BeEmpty();
    }

    [Fact]
    public void CreateSectionForTags_ShouldAssociateTags()
    {
        // Arrange
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };

        // Act
        var section = _sectionGenerator.CreateSectionForTags(tags);

        // Assert
        section.SectionTags.Should().HaveCount(2);
        section.SectionTags.Should().Contain(st => st.TagId == tag1.Id);
        section.SectionTags.Should().Contain(st => st.TagId == tag2.Id);
        section.SectionTags.All(st => st.SectionId == section.Id).Should().BeTrue();
    }
}
