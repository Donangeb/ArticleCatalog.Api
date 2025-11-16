using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Exceptions;

namespace ArticleCatalog.Tests.Unit;

/// <summary>
/// Тесты для агрегата Article (Domain слой)
/// </summary>
public class ArticleTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateArticle()
    {
        // Arrange
        var title = "Test Article";
        var tags = new[] { "tag1", "tag2" };

        // Act
        var article = Article.Create(title, tags);

        // Assert
        article.Should().NotBeNull();
        article.Title.Should().Be(title);
        article.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        article.UpdatedAt.Should().BeNull();
        article.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var title = "";
        var tags = new[] { "tag1" };

        // Act & Assert
        var act = () => Article.Create(title, tags);
        act.Should().Throw<ValidationException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ShouldThrowValidationException()
    {
        // Arrange
        var title = "   ";
        var tags = new[] { "tag1" };

        // Act & Assert
        var act = () => Article.Create(title, tags);
        act.Should().Throw<ValidationException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldThrowValidationException()
    {
        // Arrange
        var title = new string('a', 257);
        var tags = new[] { "tag1" };

        // Act & Assert
        var act = () => Article.Create(title, tags);
        act.Should().Throw<ValidationException>()
            .WithMessage("Title too long (maximum 256 characters)");
    }

    [Fact]
    public void Create_WithTooManyTags_ShouldThrowValidationException()
    {
        // Arrange
        var title = "Test Article";
        var tags = Enumerable.Range(1, 257).Select(i => $"tag{i}");

        // Act & Assert
        var act = () => Article.Create(title, tags);
        act.Should().Throw<ValidationException>()
            .WithMessage("Too many tags (maximum 256 tags)");
    }

    [Fact]
    public void Create_WithDuplicateTags_ShouldThrowValidationException()
    {
        // Arrange
        var title = "Test Article";
        var tags = new[] { "tag1", "TAG1", "tag2" };

        // Act & Assert
        var act = () => Article.Create(title, tags);
        act.Should().Throw<ValidationException>()
            .WithMessage("Duplicate tags are not allowed");
    }

    [Fact]
    public void Create_ShouldTrimTitle()
    {
        // Arrange
        var title = "  Test Article  ";
        var tags = new[] { "tag1" };

        // Act
        var article = Article.Create(title, tags);

        // Assert
        article.Title.Should().Be("Test Article");
    }

    [Fact]
    public void SetTags_ForNewArticle_ShouldAddArticleCreatedEvent()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1" });
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag1, tag2 };
        var tagIds = tags.Select(t => t.Id);

        // Act
        article.SetTags(tagIds, tags, isNewArticle: true);

        // Assert
        article.DomainEvents.Should().ContainSingle(e => e is ArticleCreatedEvent);
        var createdEvent = article.DomainEvents.OfType<ArticleCreatedEvent>().Single();
        createdEvent.ArticleId.Should().Be(article.Id);
        createdEvent.Title.Should().Be("Test Article");
        createdEvent.TagNames.Should().Contain("tag1", "tag2");
    }

    [Fact]
    public void SetTags_ForExistingArticle_ShouldAddArticleTagsChangedEvent()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1" });
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        article.SetTags(new[] { tag1.Id }, new[] { tag1 }, isNewArticle: true);
        article.ClearDomainEvents();

        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tags = new[] { tag2 };
        var tagIds = tags.Select(t => t.Id);

        // Act
        article.SetTags(tagIds, tags, isNewArticle: false);

        // Assert
        article.DomainEvents.Should().ContainSingle(e => e is ArticleTagsChangedEvent);
        var changedEvent = article.DomainEvents.OfType<ArticleTagsChangedEvent>().Single();
        changedEvent.ArticleId.Should().Be(article.Id);
        changedEvent.NewTagNames.Should().Contain("tag2");
    }

    [Fact]
    public void SetTags_ShouldSetCorrectPositions()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1", "tag2", "tag3" });
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tag3 = new Tag { Id = Guid.NewGuid(), Name = "tag3" };
        var tags = new[] { tag1, tag2, tag3 };
        var tagIds = tags.Select(t => t.Id);

        // Act
        article.SetTags(tagIds, tags, isNewArticle: true);

        // Assert
        article.ArticleTags.Should().HaveCount(3);
        article.ArticleTags.ElementAt(0).Position.Should().Be(0);
        article.ArticleTags.ElementAt(1).Position.Should().Be(1);
        article.ArticleTags.ElementAt(2).Position.Should().Be(2);
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitle()
    {
        // Arrange
        var article = Article.Create("Old Title", new[] { "tag1" });
        var newTitle = "New Title";

        // Act
        article.UpdateTitle(newTitle);

        // Assert
        article.Title.Should().Be(newTitle);
        article.UpdatedAt.Should().NotBeNull();
        article.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateTitle_WithInvalidTitle_ShouldThrowValidationException()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1" });

        // Act & Assert
        var act = () => article.UpdateTitle("");
        act.Should().Throw<ValidationException>()
            .WithMessage("Title is required");
    }

    [Fact]
    public void MarkAsDeleted_ShouldAddArticleDeletedEvent()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
        var tagNames = new[] { "tag1", "tag2" };

        // Act
        article.MarkAsDeleted(tagNames);

        // Assert
        article.DomainEvents.Should().ContainSingle(e => e is ArticleDeletedEvent);
        var deletedEvent = article.DomainEvents.OfType<ArticleDeletedEvent>().Single();
        deletedEvent.ArticleId.Should().Be(article.Id);
        deletedEvent.TagNames.Should().Contain("tag1", "tag2");
    }

    [Fact]
    public void GetTagNames_ShouldReturnTagsInCorrectOrder()
    {
        // Arrange
        var article = Article.Create("Test Article", new[] { "tag1", "tag2", "tag3" });
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        var tag3 = new Tag { Id = Guid.NewGuid(), Name = "tag3" };
        var tags = new[] { tag1, tag2, tag3 };
        var tagIds = tags.Select(t => t.Id);
        article.SetTags(tagIds, tags, isNewArticle: true);

        // Act
        var tagNames = article.GetTagNames(tags);

        // Assert
        tagNames.Should().HaveCount(3);
        tagNames[0].Should().Be("tag1");
        tagNames[1].Should().Be("tag2");
        tagNames[2].Should().Be("tag3");
    }
}

