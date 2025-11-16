using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.Tests.Unit;

/// <summary>
/// Тесты для Value Object TagSetKey (Domain слой)
/// </summary>
public class TagSetKeyTests
{
    [Fact]
    public void Create_WithTags_ShouldNormalizeAndSort()
    {
        // Arrange
        var tags = new[] { "TagC", "TagA", "TagB" };

        // Act
        var tagSetKey = TagSetKey.Create(tags);

        // Assert
        tagSetKey.Value.Should().Be("taga|tagb|tagc");
    }

    [Fact]
    public void Create_WithDifferentCase_ShouldBeEqual()
    {
        // Arrange
        var tags1 = new[] { "TagA", "TagB" };
        var tags2 = new[] { "taga", "tagb" };

        // Act
        var key1 = TagSetKey.Create(tags1);
        var key2 = TagSetKey.Create(tags2);

        // Assert
        key1.Should().Be(key2);
        key1.Value.Should().Be(key2.Value);
    }

    [Fact]
    public void Create_WithWhitespace_ShouldTrim()
    {
        // Arrange
        var tags = new[] { "  TagA  ", "  TagB  " };

        // Act
        var tagSetKey = TagSetKey.Create(tags);

        // Assert
        tagSetKey.Value.Should().Be("taga|tagb");
    }

    [Fact]
    public void Create_WithSameTagsDifferentOrder_ShouldBeEqual()
    {
        // Arrange
        var tags1 = new[] { "TagA", "TagB", "TagC" };
        var tags2 = new[] { "TagC", "TagA", "TagB" };

        // Act
        var key1 = TagSetKey.Create(tags1);
        var key2 = TagSetKey.Create(tags2);

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void Create_WithEmptyTags_ShouldCreateEmptyKey()
    {
        // Arrange
        var tags = Array.Empty<string>();

        // Act
        var tagSetKey = TagSetKey.Create(tags);

        // Assert
        tagSetKey.Value.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithSingleTag_ShouldCreateKey()
    {
        // Arrange
        var tags = new[] { "TagA" };

        // Act
        var tagSetKey = TagSetKey.Create(tags);

        // Assert
        tagSetKey.Value.Should().Be("taga");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var tags = new[] { "TagA", "TagB" };
        var tagSetKey = TagSetKey.Create(tags);

        // Act
        var result = tagSetKey.ToString();

        // Assert
        result.Should().Be(tagSetKey.Value);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var tags1 = new[] { "TagA", "TagB" };
        var tags2 = new[] { "TagA", "TagB" };

        // Act
        var key1 = TagSetKey.Create(tags1);
        var key2 = TagSetKey.Create(tags2);

        // Assert
        key1.Should().Be(key2);
        (key1 == key2).Should().BeTrue();
        (key1 != key2).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var tags1 = new[] { "TagA", "TagB" };
        var tags2 = new[] { "TagA", "TagC" };

        // Act
        var key1 = TagSetKey.Create(tags1);
        var key2 = TagSetKey.Create(tags2);

        // Assert
        key1.Should().NotBe(key2);
        (key1 == key2).Should().BeFalse();
        (key1 != key2).Should().BeTrue();
    }
}

