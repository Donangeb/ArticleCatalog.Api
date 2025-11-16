using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.DomainServices;

/// <summary>
/// Реализация доменного сервиса для создания разделов
/// </summary>
public class SectionGenerator : ISectionGenerator
{
    public Section CreateSectionForTags(IEnumerable<Tag> tags)
    {
        return Section.Create(tags);
    }
}
