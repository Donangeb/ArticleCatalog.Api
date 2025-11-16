using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;
using ArticleCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для агрегата Section
/// </summary>
public class SectionRepository : ISectionRepository
{
    private readonly ArticleCatalogDbContext _db;

    public SectionRepository(ArticleCatalogDbContext db) => _db = db;

    public Task<Section?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Section
           .Include(s => s.SectionTags)
           .ThenInclude(st => st.Tag)
           .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<Section?> GetByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default) =>
        _db.Section
           .Include(s => s.SectionTags)
           .ThenInclude(st => st.Tag)
           .FirstOrDefaultAsync(s => s.TagSetKey == tagSetKey.Value, cancellationToken);

    public Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.Section
           .Include(s => s.SectionTags)
           .ThenInclude(st => st.Tag)
           .ToListAsync(cancellationToken)
           .ContinueWith(t => (IReadOnlyList<Section>)t.Result, cancellationToken);

    public Task AddAsync(Section section, CancellationToken cancellationToken = default)
    {
        _db.Section.Add(section);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Section section, CancellationToken cancellationToken = default)
    {
        _db.Section.Update(section);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Section section, CancellationToken cancellationToken = default)
    {
        _db.Section.Remove(section);
        return Task.CompletedTask;
    }

    public async Task<int> CountArticlesByTagSetKeyAsync(TagSetKey tagSetKey, CancellationToken cancellationToken = default)
    {
        var articles = await _db.Article
            .Include(a => a.ArticleTags)
            .ThenInclude(at => at.Tag)
            .ToListAsync(cancellationToken);

        var matchingArticles = articles
            .Where(a =>
            {
                var articleTagNames = a.ArticleTags
                    .OrderBy(at => at.Tag.Name)
                    .Select(at => at.Tag.Name)
                    .ToList();
                var articleTagSetKey = TagSetKey.Create(articleTagNames);
                return articleTagSetKey.Value == tagSetKey.Value;
            })
            .Count();

        return matchingArticles;
    }
}
