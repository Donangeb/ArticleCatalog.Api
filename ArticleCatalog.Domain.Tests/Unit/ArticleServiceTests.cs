using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Tests.Helpers;

namespace ArticleCatalog.Domain.Tests.Unit;

/// <summary>
/// Тесты для доменной логики создания статей
/// </summary>
public class ArticleServiceTests
{
    [Fact]
    public void CreateArticle_Should_Set_CreatedAt_And_Section_Automatically()
    {
        // Arrange
        var title = "Test Article";
        var tagNames = new[] { "tag1", "tag2" };
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tags = new[] { tag1, tag2 };
        var tagIds = new[] { tag1.Id, tag2.Id };

        // Act
        var article = Article.Create(title, tagNames);
        var beforeSetTags = DateTimeOffset.UtcNow;
        article.SetTags(tagIds, tags, isNewArticle: true);
        var afterSetTags = DateTimeOffset.UtcNow;

        // Assert
        // Проверяем, что CreatedAt установлен автоматически при создании статьи
        article.CreatedAt.Should().BeCloseTo(beforeSetTags, TimeSpan.FromSeconds(1));
        article.CreatedAt.Should().BeBefore(afterSetTags);
        article.CreatedAt.Should().BeAfter(beforeSetTags.AddSeconds(-1));

        // Проверяем, что доменное событие ArticleCreatedEvent опубликовано
        // Это событие будет обработано ArticleCreatedEventHandler в Application слое,
        // который автоматически создаст Section для набора тегов
        article.DomainEvents.Should().ContainSingle();
        var domainEvent = article.DomainEvents.First();
        domainEvent.Should().BeOfType<ArticleCreatedEvent>();
        
        var articleCreatedEvent = domainEvent as ArticleCreatedEvent;
        articleCreatedEvent.Should().NotBeNull();
        articleCreatedEvent!.ArticleId.Should().Be(article.Id);
        articleCreatedEvent.Title.Should().Be(title);
        articleCreatedEvent.TagNames.Should().Contain("tag1", "tag2");
        articleCreatedEvent.CreatedAt.Should().Be(article.CreatedAt);
        
        // Проверяем, что теги установлены
        article.ArticleTags.Should().HaveCount(2);
    }

    [Fact]
    public void CreateArticle_WithTags_ShouldPublishArticleCreatedEvent()
    {
        // Arrange
        var title = "Another Article";
        var tagNames = new[] { "tech", "programming" };
        var tag1 = TagTestHelper.CreateTag("tech");
        var tag2 = TagTestHelper.CreateTag("programming");
        var tags = new[] { tag1, tag2 };
        var tagIds = new[] { tag1.Id, tag2.Id };

        // Act
        var article = Article.Create(title, tagNames);
        article.SetTags(tagIds, tags, isNewArticle: true);

        // Assert
        article.DomainEvents.Should().HaveCount(1);
        var @event = article.DomainEvents.First() as ArticleCreatedEvent;
        @event.Should().NotBeNull();
        @event!.TagNames.Should().Equal("tech", "programming");
        @event.CreatedAt.Should().Be(article.CreatedAt);
    }
}
