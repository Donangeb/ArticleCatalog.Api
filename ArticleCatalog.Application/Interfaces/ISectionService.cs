using ArticleCatalog.Application.DTOs;

namespace ArticleCatalog.Application.Interfaces;

public interface ISectionService
{
    Task<IReadOnlyList<SectionDto>> GetSectionsAsync();
    Task<IReadOnlyList<ArticleDto>> GetSectionArticlesAsync(Guid sectionId);
}
