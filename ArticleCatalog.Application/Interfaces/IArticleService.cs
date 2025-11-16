using ArticleCatalog.Application.DTOs;

namespace ArticleCatalog.Application.Interfaces;
public interface IArticleService
{
    Task<ArticleDto> CreateAsync(CreateArticleRequest request);
    Task<ArticleDto> UpdateAsync(Guid id, UpdateArticleRequest request);
    Task<ArticleDto> GetAsync(Guid id);
    Task DeleteAsync(Guid id);
}
