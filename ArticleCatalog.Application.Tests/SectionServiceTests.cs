using ArticleCatalog.Application.Services;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Exceptions;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.Tests.Helpers;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Application.Tests;

/// <summary>
/// Тесты для SectionService (Application слой)
/// </summary>
public class SectionServiceTests
{
    private readonly Mock<ISectionRepository> _sectionRepositoryMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly SectionService _service;

    public SectionServiceTests()
    {
        _sectionRepositoryMock = new Mock<ISectionRepository>();
        _articleRepositoryMock = new Mock<IArticleRepository>();

        _service = new SectionService(
            _sectionRepositoryMock.Object,
            _articleRepositoryMock.Object
        );
    }

    [Fact]
    public async Task GetSectionsAsync_WithExistingSections_ShouldReturnOrderedSections()
    {
        // Arrange
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tag3 = TagTestHelper.CreateTag("tag3");

        var section1 = Section.Create(new[] { tag1, tag2 });
        var section2 = Section.Create(new[] { tag3 });
        
        // Устанавливаем навигационные свойства для SectionTags
        foreach (var sectionTag in section1.SectionTags)
        {
            sectionTag.Tag = sectionTag.TagId == tag1.Id ? tag1 : tag2;
        }
        foreach (var sectionTag in section2.SectionTags)
        {
            sectionTag.Tag = tag3;
        }

        var sections = new List<Section> { section1, section2 };

        _sectionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sections);
        _sectionRepositoryMock.Setup(x => x.CountArticlesByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag1", "tag2" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        _sectionRepositoryMock.Setup(x => x.CountArticlesByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag3" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _service.GetSectionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].ArticlesCount.Should().Be(10); // Первая секция должна быть с большим количеством статей
        result[1].ArticlesCount.Should().Be(5);
        result[0].TagCount.Should().Be(2);
        result[1].TagCount.Should().Be(1);
        result[0].Tags.Should().Contain("tag1", "tag2");
        result[1].Tags.Should().Contain("tag3");
    }

    [Fact]
    public async Task GetSectionsAsync_WithNoSections_ShouldReturnEmptyList()
    {
        // Arrange
        _sectionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Section>());

        // Act
        var result = await _service.GetSectionsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSectionsAsync_ShouldOrderSectionsByArticlesCountDescending()
    {
        // Arrange
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tag3 = TagTestHelper.CreateTag("tag3");

        var section1 = Section.Create(new[] { tag1 });
        var section2 = Section.Create(new[] { tag2 });
        var section3 = Section.Create(new[] { tag3 });

        foreach (var sectionTag in section1.SectionTags)
        {
            sectionTag.Tag = tag1;
        }
        foreach (var sectionTag in section2.SectionTags)
        {
            sectionTag.Tag = tag2;
        }
        foreach (var sectionTag in section3.SectionTags)
        {
            sectionTag.Tag = tag3;
        }

        var sections = new List<Section> { section1, section2, section3 };

        _sectionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sections);
        _sectionRepositoryMock.Setup(x => x.CountArticlesByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag1" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _sectionRepositoryMock.Setup(x => x.CountArticlesByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag2" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);
        _sectionRepositoryMock.Setup(x => x.CountArticlesByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag3" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _service.GetSectionsAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].ArticlesCount.Should().Be(15); // Наибольшее количество
        result[1].ArticlesCount.Should().Be(10);
        result[2].ArticlesCount.Should().Be(5);  // Наименьшее количество
    }

    [Fact]
    public async Task GetSectionArticlesAsync_WithValidSectionId_ShouldReturnOrderedArticles()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        
        var section = Section.Create(new[] { tag1, tag2 });
        foreach (var sectionTag in section.SectionTags)
        {
            sectionTag.Tag = sectionTag.TagId == tag1.Id ? tag1 : tag2;
        }

        var article1 = Article.Create("Article 1", new[] { "tag1", "tag2" });
        var article2 = Article.Create("Article 2", new[] { "tag1", "tag2" });
        
        var tag1Id = tag1.Id;
        var tag2Id = tag2.Id;
        var tags = new[] { tag1, tag2 };
        
        article1.SetTags(new[] { tag1Id, tag2Id }, tags, isNewArticle: true);
        article2.SetTags(new[] { tag1Id, tag2Id }, tags, isNewArticle: true);
        
        // Устанавливаем навигационные свойства для ArticleTags
        foreach (var articleTag in article1.ArticleTags)
        {
            articleTag.Tag = tags.First(t => t.Id == articleTag.TagId);
        }
        foreach (var articleTag in article2.ArticleTags)
        {
            articleTag.Tag = tags.First(t => t.Id == articleTag.TagId);
        }

        // Обновляем вторую статью, чтобы она была новее
        article2.MarkAsUpdated();

        var articles = new List<Article> { article1, article2 };

        _sectionRepositoryMock.Setup(x => x.GetByIdAsync(sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _articleRepositoryMock.Setup(x => x.GetByTagSetKeyAsync(
            It.Is<TagSetKey>(k => k.Value == TagSetKey.Create(new[] { "tag1", "tag2" }).Value),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(articles);

        // Act
        var result = await _service.GetSectionArticlesAsync(sectionId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Article 2"); // Обновленная статья должна быть первой
        result[1].Title.Should().Be("Article 1");
        result[0].Tags.Should().Contain("tag1", "tag2");
        result[1].Tags.Should().Contain("tag1", "tag2");
    }

    [Fact]
    public async Task GetSectionArticlesAsync_WithNonExistentSectionId_ShouldThrowNotFoundException()
    {
        // Arrange
        var sectionId = Guid.NewGuid();

        _sectionRepositoryMock.Setup(x => x.GetByIdAsync(sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Section?)null);

        // Act & Assert
        var act = async () => await _service.GetSectionArticlesAsync(sectionId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Section {sectionId} not found");
    }

    [Fact]
    public async Task GetSectionArticlesAsync_WithNoArticles_ShouldReturnEmptyList()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        
        var section = Section.Create(new[] { tag1, tag2 });
        foreach (var sectionTag in section.SectionTags)
        {
            sectionTag.Tag = sectionTag.TagId == tag1.Id ? tag1 : tag2;
        }

        _sectionRepositoryMock.Setup(x => x.GetByIdAsync(sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _articleRepositoryMock.Setup(x => x.GetByTagSetKeyAsync(
            It.IsAny<TagSetKey>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Article>());

        // Act
        var result = await _service.GetSectionArticlesAsync(sectionId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSectionArticlesAsync_ShouldSortArticlesByUpdatedAtDescending()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag("tag1");
        var section = Section.Create(new[] { tag1 });
        foreach (var sectionTag in section.SectionTags)
        {
            sectionTag.Tag = tag1;
        }

        var now = DateTimeOffset.UtcNow;
        var article1 = Article.Create("Article 1", new[] { "tag1" });
        var article2 = Article.Create("Article 2", new[] { "tag1" });
        var article3 = Article.Create("Article 3", new[] { "tag1" });

        var tags = new[] { tag1 };
        article1.SetTags(new[] { tag1.Id }, tags, isNewArticle: true);
        article2.SetTags(new[] { tag1.Id }, tags, isNewArticle: true);
        article3.SetTags(new[] { tag1.Id }, tags, isNewArticle: true);

        // Устанавливаем навигационные свойства
        foreach (var articleTag in article1.ArticleTags)
        {
            articleTag.Tag = tag1;
        }
        foreach (var articleTag in article2.ArticleTags)
        {
            articleTag.Tag = tag1;
        }
        foreach (var articleTag in article3.ArticleTags)
        {
            articleTag.Tag = tag1;
        }

        // Обновляем статьи в разное время
        article2.MarkAsUpdated();
        await Task.Delay(10); // Небольшая задержка для разных временных меток
        article3.MarkAsUpdated();

        var articles = new List<Article> { article1, article2, article3 };

        _sectionRepositoryMock.Setup(x => x.GetByIdAsync(sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _articleRepositoryMock.Setup(x => x.GetByTagSetKeyAsync(
            It.IsAny<TagSetKey>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(articles);

        // Act
        var result = await _service.GetSectionArticlesAsync(sectionId);

        // Assert
        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Article 3"); // Самая новая
        result[1].Title.Should().Be("Article 2");
        result[2].Title.Should().Be("Article 1"); // Самая старая
    }

    [Fact]
    public async Task GetSectionArticlesAsync_ShouldOrderTagsByPosition()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var tag1 = TagTestHelper.CreateTag("tag1");
        var tag2 = TagTestHelper.CreateTag("tag2");
        var tag3 = TagTestHelper.CreateTag("tag3");
        
        var section = Section.Create(new[] { tag1, tag2, tag3 });
        foreach (var sectionTag in section.SectionTags)
        {
            sectionTag.Tag = sectionTag.TagId == tag1.Id ? tag1 : 
                            sectionTag.TagId == tag2.Id ? tag2 : tag3;
        }

        var article = Article.Create("Test Article", new[] { "tag3", "tag1", "tag2" });
        var tags = new[] { tag1, tag2, tag3 };
        article.SetTags(new[] { tag3.Id, tag1.Id, tag2.Id }, tags, isNewArticle: true);
        
        foreach (var articleTag in article.ArticleTags)
        {
            articleTag.Tag = tags.First(t => t.Id == articleTag.TagId);
        }

        _sectionRepositoryMock.Setup(x => x.GetByIdAsync(sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(section);
        _articleRepositoryMock.Setup(x => x.GetByTagSetKeyAsync(
            It.IsAny<TagSetKey>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Article> { article });

        // Act
        var result = await _service.GetSectionArticlesAsync(sectionId);

        // Assert
        result.Should().HaveCount(1);
        result[0].Tags.Should().HaveCount(3);
        result[0].Tags[0].Should().Be("tag3"); // Position 0
        result[0].Tags[1].Should().Be("tag1"); // Position 1
        result[0].Tags[2].Should().Be("tag2"); // Position 2
    }
}

