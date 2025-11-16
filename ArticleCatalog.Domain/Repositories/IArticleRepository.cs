using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с агрегатом Article
/// </summary>
public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Article>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Article>> GetByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default);
    Task AddAsync(Article article, CancellationToken cancellationToken = default);
    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);
    Task RemoveAsync(Article article, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}

