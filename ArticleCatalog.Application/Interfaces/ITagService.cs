namespace ArticleCatalog.Application.Interfaces;
public interface ITagService
{
    Task<Guid> GetOrCreateAsync(string tagName);
    Task<IReadOnlyList<Guid>> GetOrCreateManyAsync(IEnumerable<string> tagNames);
}
