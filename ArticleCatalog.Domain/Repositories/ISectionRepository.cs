using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Domain.Repositories;

/// <summary>
/// Репозиторий для работы с агрегатом Section
/// </summary>
public interface ISectionRepository
{
    Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Section?> GetByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Section section, CancellationToken cancellationToken = default);
    Task UpdateAsync(Section section, CancellationToken cancellationToken = default);
    Task RemoveAsync(Section section, CancellationToken cancellationToken = default);
    Task<int> CountArticlesByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default);
}

