using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;
using ArticleCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для агрегата Article
/// </summary>
public class ArticleRepository : IArticleRepository
{
    private readonly ArticleCatalogDbContext _db;

    public ArticleRepository(ArticleCatalogDbContext db) => _db = db;

    public Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Article
           .Include(a => a.ArticleTags)
           .ThenInclude(t => t.Tag)
           .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task AddAsync(Article article, CancellationToken cancellationToken = default)
    {
        _db.Article.Add(article);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
    {
        _db.Article.Update(article);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Article article, CancellationToken cancellationToken = default)
    {
        _db.Article.Remove(article);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Article.AnyAsync(a => a.Id == id, cancellationToken);

    public Task<IReadOnlyList<Article>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Article
           .Include(a => a.ArticleTags)
           .ThenInclude(t => t.Tag)
           .ToListAsync(cancellationToken)
           .ContinueWith(t => (IReadOnlyList<Article>)t.Result, cancellationToken);

    public async Task<IReadOnlyList<Article>> GetByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default)
    {
        // Оптимизированный запрос: используем TagSetKey из БД вместо загрузки всех статей
        return await _db.Article
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .Where(a => a.TagSetKey == tagSetKey.Value)
            .ToListAsync(cancellationToken);
    }
}
