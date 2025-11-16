using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Application.Services;
using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Exceptions;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.Tests;

/// <summary>
/// Тесты для ArticleService (Application слой)
/// </summary>
public class ArticleServiceTests
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<ITagService> _tagServiceMock;
    private readonly Mock<ILogger<ArticleService>> _loggerMock;
    private readonly ArticleService _service;

    public ArticleServiceTests()
    {
        _articleRepositoryMock = new Mock<IArticleRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _tagServiceMock = new Mock<ITagService>();
        _loggerMock = new Mock<ILogger<ArticleService>>();

        _service = new ArticleService(
            _articleRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _eventDispatcherMock.Object,
            _tagServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateArticle()
    {
        // Arrange
        var request = new CreateArticleRequest("Test Article", new List<string> { "tag1", "tag2" });
        var tag1Id = Guid.NewGuid();
        var tag2Id = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag(tag1Id, "tag1");
        var tag2 = TagTestHelper.CreateTag(tag2Id, "tag2");

        Article? createdArticle = null;
        
        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(new[] { tag1Id, tag2Id });
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag1, tag2 });
        _articleRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()))
            .Callback<Article, CancellationToken>((article, _) => { createdArticle = article; });
        _articleRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                if (createdArticle != null && createdArticle.Id == id)
                {
                    // Устанавливаем навигационное свойство Tag для каждого ArticleTag
                    var tags = new[] { tag1, tag2 };
                    foreach (var articleTag in createdArticle.ArticleTags)
                    {
                        articleTag.Tag = tags.First(t => t.Id == articleTag.TagId);
                    }
                    return createdArticle;
                }
                var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
                var tagsArray = new[] { tag1, tag2 };
                article.SetTags(new[] { tag1Id, tag2Id }, tagsArray, isNewArticle: true);
                // Устанавливаем навигационное свойство Tag для каждого ArticleTag
                foreach (var articleTag in article.ArticleTags)
                {
                    articleTag.Tag = tagsArray.First(t => t.Id == articleTag.TagId);
                }
                return article;
            });
        _eventDispatcherMock.Setup(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Article");
        result.Tags.Should().Contain("tag1", "tag2");
        _articleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTitle_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateArticleRequest("", new List<string> { "tag1" });

        // Act & Assert
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateArticle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new List<string> { "tag1", "tag3" });
        var existingArticle = Article.Create("Old Title", new[] { "tag1", "tag2" });
        var tag1Id = Guid.NewGuid();
        var tag3Id = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag(tag1Id, "tag1");
        var tag3 = TagTestHelper.CreateTag(tag3Id, "tag3");
        // Устанавливаем начальные теги для existingArticle
        var initialTag1 = TagTestHelper.CreateTag(tag1Id, "tag1");
        var initialTag2 = TagTestHelper.CreateTag("tag2");
        var initialTags = new[] { initialTag1, initialTag2 };
        existingArticle.SetTags(new[] { tag1Id, initialTag2.Id }, initialTags, isNewArticle: true);
        // Устанавливаем навигационное свойство Tag для каждого ArticleTag
        foreach (var articleTag in existingArticle.ArticleTags)
        {
            articleTag.Tag = initialTags.First(t => t.Id == articleTag.TagId);
        }

        var callCount = 0;
        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Первый вызов - возвращаем существующую статью
                    return existingArticle;
                }
                else
                {
                    // Второй вызов (из BuildDto) - возвращаем обновленную статью с установленными Tag
                    var updatedTags = new[] { tag1, tag3 };
                    // Устанавливаем навигационное свойство Tag для каждого ArticleTag
                    foreach (var articleTag in existingArticle.ArticleTags)
                    {
                        articleTag.Tag = updatedTags.First(t => t.Id == articleTag.TagId);
                    }
                    return existingArticle;
                }
            });
        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(new[] { tag1Id, tag3Id });
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { tag1, tag3 });
        _eventDispatcherMock.Setup(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(articleId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        existingArticle.Title.Should().Be("Updated Title");
        _articleRepositoryMock.Verify(x => x.UpdateAsync(existingArticle, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentArticle_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new List<string> { "tag1" });

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);

        // Act & Assert
        var act = async () => await _service.UpdateAsync(articleId, request);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article {articleId} not found");
    }

    [Fact]
    public async Task GetAsync_WithValidId_ShouldReturnArticle()
    {
        // Arrange
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tags = new[] { tag1, tag2 };
        var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
        article.SetTags(new[] { tag1.Id, tag2.Id }, tags, isNewArticle: true);
        var articleId = article.Id; // Используем ID созданной статьи

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => id == articleId || id == article.Id ? article : null);
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _service.GetAsync(articleId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Article");
        result.Tags.Should().HaveCount(2);
        result.Tags.Should().Contain("tag1", "tag2");
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);

        // Act & Assert
        var act = async () => await _service.GetAsync(articleId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article {articleId} not found");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteArticle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tags = new[] { tag1, tag2 };
        article.SetTags(new[] { tag1.Id, tag2.Id }, tags, isNewArticle: true);
        // Устанавливаем навигационное свойство Tag для каждого ArticleTag
        foreach (var articleTag in article.ArticleTags)
        {
            articleTag.Tag = tags.First(t => t.Id == articleTag.TagId);
        }

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        await _service.DeleteAsync(articleId);

        // Assert
        _articleRepositoryMock.Verify(x => x.RemoveAsync(article, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), It.IsAny<CancellationToken>()), Times.Once);
        article.DomainEvents.Should().ContainSingle(e => e is Domain.Events.ArticleDeletedEvent);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);

        // Act & Assert
        var act = async () => await _service.DeleteAsync(articleId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article {articleId} not found");
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTags_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateArticleRequest("Test Article", new List<string>());

        // Настраиваем моки для пустого списка тегов
        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(Array.Empty<Guid>());
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Tag>());

        // Act & Assert
        // Валидация должна произойти в Article.Create, который проверяет tagNames
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateTags_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateArticleRequest("Test Article", new List<string> { "tag1", "tag1" });

        // Act & Assert
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithTooLongTitle_ShouldThrowValidationException()
    {
        // Arrange
        var longTitle = new string('A', 257); // Превышает лимит в 256 символов
        var request = new CreateArticleRequest(longTitle, new List<string> { "tag1" });

        // Act & Assert
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidTitle_ShouldThrowValidationException()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var article = Article.Create("Old Title", new[] { "tag1" });
        var tag1 = TagTestHelper.CreateTag("tag1");
        article.SetTags(new[] { tag1.Id }, new[] { tag1 }, isNewArticle: true);
        foreach (var articleTag in article.ArticleTags)
        {
            articleTag.Tag = tag1;
        }

        var request = new UpdateArticleRequest("", new List<string> { "tag1" });

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act & Assert
        var act = async () => await _service.UpdateAsync(articleId, request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyTags_ShouldThrowValidationException()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var article = Article.Create("Old Title", new[] { "tag1" });
        var tag1 = TagTestHelper.CreateTag("tag1");
        article.SetTags(new[] { tag1.Id }, new[] { tag1 }, isNewArticle: true);
        foreach (var articleTag in article.ArticleTags)
        {
            articleTag.Tag = tag1;
        }

        var request = new UpdateArticleRequest("New Title", new List<string>());

        // Настраиваем моки для пустого списка тегов
        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(Array.Empty<Guid>());
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Tag>());

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(article);

        // Act & Assert
        // Валидация должна произойти в SetTags, который проверяет количество тегов
        var act = async () => await _service.UpdateAsync(articleId, request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceTitle_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateArticleRequest("   ", new List<string> { "tag1" });

        // Act & Assert
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }
}

