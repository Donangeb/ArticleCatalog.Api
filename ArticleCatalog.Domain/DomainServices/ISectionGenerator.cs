using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.DomainServices;

/// <summary>
/// Доменный сервис для создания разделов на основе набора тегов
/// </summary>
public interface ISectionGenerator
{
    /// <summary>
    /// Создает раздел для указанного набора тегов
    /// </summary>
    Section CreateSectionForTags(IEnumerable<Tag> tags);
}
