using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Application.Services;
using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Exceptions;
using ArticleCatalog.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

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
        var request = new CreateArticleRequest("Test Article", new[] { "tag1", "tag2" });
        var tag1Id = Guid.NewGuid();
        var tag2Id = Guid.NewGuid();
        var tag1 = new Tag { Id = tag1Id, Name = "tag1" };
        var tag2 = new Tag { Id = tag2Id, Name = "tag2" };

        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(new[] { tag1Id, tag2Id });
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new[] { tag1, tag2 });
        _articleRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
                var tags = new[] { tag1, tag2 };
                article.SetTags(new[] { tag1Id, tag2Id }, tags, isNewArticle: true);
                return article;
            });
        _eventDispatcherMock.Setup(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Article");
        result.Tags.Should().Contain("tag1", "tag2");
        _articleRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Article>(), default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTitle_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateArticleRequest("", new[] { "tag1" });

        // Act & Assert
        var act = async () => await _service.CreateAsync(request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateArticle()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new[] { "tag1", "tag3" });
        var existingArticle = Article.Create("Old Title", new[] { "tag1", "tag2" });
        var tag1Id = Guid.NewGuid();
        var tag3Id = Guid.NewGuid();
        var tag1 = new Tag { Id = tag1Id, Name = "tag1" };
        var tag3 = new Tag { Id = tag3Id, Name = "tag3" };

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
            .ReturnsAsync(existingArticle);
        _tagServiceMock.Setup(x => x.GetOrCreateManyAsync(request.Tags))
            .ReturnsAsync(new[] { tag1Id, tag3Id });
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new[] { tag1, tag3 });
        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var article = Article.Create("Updated Title", new[] { "tag1", "tag3" });
                var tags = new[] { tag1, tag3 };
                article.SetTags(new[] { tag1Id, tag3Id }, tags, isNewArticle: false);
                return article;
            });
        _eventDispatcherMock.Setup(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(articleId, request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Updated Title");
        existingArticle.Title.Should().Be("Updated Title");
        _articleRepositoryMock.Verify(x => x.UpdateAsync(existingArticle, default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentArticle_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();
        var request = new UpdateArticleRequest("Updated Title", new[] { "tag1" });

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
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
        var articleId = Guid.NewGuid();
        var article = Article.Create("Test Article", new[] { "tag1", "tag2" });
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        article.SetTags(new[] { tag1.Id, tag2.Id }, new[] { tag1, tag2 }, isNewArticle: true);

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
            .ReturnsAsync(article);

        // Act
        var result = await _service.GetAsync(articleId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(articleId);
        result.Title.Should().Be("Test Article");
    }

    [Fact]
    public async Task GetAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
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
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "tag1" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "tag2" };
        article.SetTags(new[] { tag1.Id, tag2.Id }, new[] { tag1, tag2 }, isNewArticle: true);

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
            .ReturnsAsync(article);
        _tagRepositoryMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new[] { tag1, tag2 });

        // Act
        await _service.DeleteAsync(articleId);

        // Assert
        _articleRepositoryMock.Verify(x => x.RemoveAsync(article, default), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        _eventDispatcherMock.Verify(x => x.DispatchEventsAsync(It.IsAny<IEnumerable<AggregateRoot<Guid>>>(), default), Times.Once);
        article.DomainEvents.Should().ContainSingle(e => e is Domain.Events.ArticleDeletedEvent);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldThrowNotFoundException()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        _articleRepositoryMock.Setup(x => x.GetByIdAsync(articleId, default))
            .ReturnsAsync((Article?)null);

        // Act & Assert
        var act = async () => await _service.DeleteAsync(articleId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Article {articleId} not found");
    }
}

