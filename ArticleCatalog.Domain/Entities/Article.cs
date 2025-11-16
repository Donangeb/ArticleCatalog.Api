using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Exceptions;

namespace ArticleCatalog.Domain.Entities;

/// <summary>
/// Агрегат Article - корень агрегата для статей
/// </summary>
public class Article : AggregateRoot<Guid>
{
    private readonly List<ArticleTag> _articleTags = new();

    public string Title { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<ArticleTag> ArticleTags => _articleTags.AsReadOnly();

    // Для EF Core - должен быть доступен для отражения
    protected Article() : base() { }

    private Article(Guid id, string title, DateTimeOffset createdAt) : base(id)
    {
        Title = title;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Фабричный метод для создания новой статьи
    /// </summary>
    public static Article Create(string title, IEnumerable<string> tagNames)
    {
        ValidateTitle(title);
        ValidateTagNames(tagNames);

        var article = new Article(
            Guid.NewGuid(),
            title.Trim(),
            DateTimeOffset.UtcNow
        );

        // Доменное событие будет добавлено после установки тегов через SetTags
        return article;
    }

    /// <summary>
    /// Устанавливает теги для статьи
    /// </summary>
    public void SetTags(IEnumerable<Guid> tagIds, IEnumerable<Tag> allTags, bool isNewArticle = false)
    {
        var tagIdsList = tagIds.ToList();
        ValidateTagCount(tagIdsList.Count);

        var tagDict = allTags.ToDictionary(t => t.Id);
        var tagNames = tagIdsList
            .Where(id => tagDict.ContainsKey(id))
            .Select(id => tagDict[id].Name)
            .ToList();

        _articleTags.Clear();

        for (int i = 0; i < tagIdsList.Count; i++)
        {
            if (tagDict.ContainsKey(tagIdsList[i]))
            {
                _articleTags.Add(new ArticleTag
                {
                    ArticleId = Id,
                    TagId = tagIdsList[i],
                    Position = i
                });
            }
        }

        // Публикуем доменное событие
        if (isNewArticle)
        {
            AddDomainEvent(new ArticleCreatedEvent(Id, Title, tagNames, CreatedAt));
        }
        else
        {
            AddDomainEvent(new ArticleTagsChangedEvent(Id, tagNames, DateTimeOffset.UtcNow));
        }
    }

    /// <summary>
    /// Обновляет название статьи
    /// </summary>
    public void UpdateTitle(string title)
    {
        ValidateTitle(title);
        Title = title.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Отмечает статью как обновленную
    /// </summary>
    public void MarkAsUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Получает названия тегов (для доменных событий)
    /// </summary>
    public IReadOnlyList<string> GetTagNames(IEnumerable<Tag> tags)
    {
        var tagDict = tags.ToDictionary(t => t.Id);
        return _articleTags
            .OrderBy(at => at.Position)
            .Select(at => tagDict.TryGetValue(at.TagId, out var tag) ? tag.Name : string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
    }

    /// <summary>
    /// Отмечает статью как удаленную и публикует доменное событие
    /// </summary>
    public void MarkAsDeleted(IReadOnlyList<string> tagNames)
    {
        AddDomainEvent(new ArticleDeletedEvent(Id, tagNames));
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Title is required");

        if (title.Length > 256)
            throw new ValidationException("Title too long (maximum 256 characters)");
    }

    private static void ValidateTagNames(IEnumerable<string> tagNames)
    {
        var tags = tagNames.ToList();
        ValidateTagCount(tags.Count);

        var distinctTags = tags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinctTags.Count != tags.Count)
            throw new ValidationException("Duplicate tags are not allowed");
    }

    private static void ValidateTagCount(int count)
    {
        if (count > 256)
            throw new ValidationException("Too many tags (maximum 256 tags)");
    }
}
