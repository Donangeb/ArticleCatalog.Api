using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.Entities;

/// <summary>
/// Агрегат Section - корень агрегата для разделов
/// </summary>
public class Section : AggregateRoot<Guid>
{
    private readonly List<SectionTag> _sectionTags = new();

    public string Title { get; private set; } = string.Empty;
    public string TagSetKey { get; private set; } = string.Empty;
    public int TagCount { get; private set; }

    public IReadOnlyCollection<SectionTag> SectionTags => _sectionTags.AsReadOnly();

    // Для EF Core - должен быть доступен для отражения
    protected Section() : base() { }

    private Section(Guid id, TagSetKey tagSetKey, int tagCount, string title) : base(id)
    {
        TagSetKey = tagSetKey.Value;
        TagCount = tagCount;
        Title = title;
    }

    /// <summary>
    /// Фабричный метод для создания раздела на основе набора тегов
    /// </summary>
    public static Section Create(IEnumerable<Tag> tags)
    {
        var tagList = tags.ToList();
        var tagNames = tagList.Select(t => t.Name).ToList();
        var tagSetKey = ValueObjects.TagSetKey.Create(tagNames);
        var title = BuildTitle(tagNames);

        var section = new Section(
            Guid.NewGuid(),
            tagSetKey,
            tagList.Count,
            title
        );

        foreach (var tag in tagList)
        {
            section._sectionTags.Add(new SectionTag
            {
                SectionId = section.Id,
                TagId = tag.Id
            });
        }

        return section;
    }

    /// <summary>
    /// Проверяет, соответствует ли раздел указанному набору тегов
    /// </summary>
    public bool MatchesTagSet(TagSetKey tagSetKey)
    {
        return TagSetKey == tagSetKey.Value;
    }

    private static string BuildTitle(IEnumerable<string> tags)
    {
        var title = string.Join(", ", tags.OrderBy(t => t));
        return title.Length > 1024 ? title[..1024] : title;
    }
}
