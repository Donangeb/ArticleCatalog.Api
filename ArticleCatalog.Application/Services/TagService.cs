using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Repositories;

namespace ArticleCatalog.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(ITagRepository tagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> GetOrCreateAsync(string tagName)
    {
        var existing = await _tagRepository.GetByNameAsync(tagName);

        if (existing != null)
            return existing.Id;

        var newTag = new Tag { Id = Guid.NewGuid(), Name = tagName.Trim() };
        await _tagRepository.AddAsync(newTag);
        await _unitOfWork.SaveChangesAsync();

        return newTag.Id;
    }

    public async Task<IReadOnlyList<Guid>> GetOrCreateManyAsync(IEnumerable<string> tagNames)
    {
        var result = new List<Guid>();

        foreach (var name in tagNames)
        {
            result.Add(await GetOrCreateAsync(name));
        }

        return result;
    }
}