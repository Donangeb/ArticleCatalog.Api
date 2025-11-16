using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация репозитория для сущности Tag
/// </summary>
public class TagRepository : ITagRepository
{
    private readonly ArticleCatalogDbContext _db;

    public TagRepository(ArticleCatalogDbContext db) => _db = db;

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Tag.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _db.Tag.FirstOrDefaultAsync(t => t.Name.ToLower() == name.Trim().ToLowerInvariant(), cancellationToken);

    public Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default) =>
        _db.Tag
           .Where(t => ids.Contains(t.Id))
           .ToListAsync(cancellationToken)
           .ContinueWith(t => (IReadOnlyList<Tag>)t.Result, cancellationToken);

    public Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _db.Tag.Add(tag);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _db.Tag.AnyAsync(t => t.Name.ToLower() == name.Trim().ToLowerInvariant(), cancellationToken);
}

