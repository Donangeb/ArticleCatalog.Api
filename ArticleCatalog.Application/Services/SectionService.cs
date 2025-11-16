using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Exceptions;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;

namespace ArticleCatalog.Application.Services;

public class SectionService : ISectionService, ISectionServiceInternal
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly ITagRepository _tagRepository;

    public SectionService(
        ISectionRepository sectionRepository,
        IArticleRepository articleRepository,
        ITagRepository tagRepository)
    {
        _sectionRepository = sectionRepository;
        _articleRepository = articleRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IReadOnlyList<SectionDto>> GetSectionsAsync()
    {
        var sections = await _sectionRepository.GetAllAsync();

        var result = new List<SectionDto>();

        foreach (var s in sections)
        {
            var tagSetKey = TagSetKey.Create(s.SectionTags.Select(st => st.Tag.Name));
            var articlesCount = await _sectionRepository.CountArticlesByTagSetKeyAsync(tagSetKey);

            result.Add(new SectionDto(
                s.Id,
                s.Title,
                s.TagCount,
                articlesCount,
                s.SectionTags.OrderBy(st => st.Tag.Name).Select(t => t.Tag.Name).ToList()
            ));
        }

        return result
            .OrderByDescending(s => s.ArticlesCount)
            .ToList();
    }

    public async Task<IReadOnlyList<ArticleDto>> GetSectionArticlesAsync(Guid sectionId)
    {
        var section = await _sectionRepository.GetByIdAsync(sectionId)
            ?? throw new NotFoundException($"Section {sectionId} not found");

        var sectionTagSetKey = TagSetKey.Create(section.SectionTags.Select(st => st.Tag.Name));
        
        var articles = await _articleRepository.GetByTagSetKeyAsync(sectionTagSetKey);
        
        var sortedArticles = articles
            .OrderByDescending(a => SortDate.Resolve(a.CreatedAt, a.UpdatedAt).Value)
            .ToList();

        return sortedArticles
            .Select(a => new ArticleDto(
                a.Id,
                a.Title,
                a.CreatedAt,
                a.UpdatedAt,
                a.ArticleTags.OrderBy(t => t.Position).Select(t => t.Tag.Name).ToList()
            ))
            .ToList();
    }

    public async Task AssignArticleToSectionAsync(Guid articleId)
    {
        // Этот метод больше не нужен - логика перенесена в обработчики доменных событий
        // Оставляем пустую реализацию для обратной совместимости
        await Task.CompletedTask;
    }

    public async Task CleanupSectionsAsync()
    {
        // Этот метод больше не нужен - логика перенесена в обработчики доменных событий
        // Оставляем пустую реализацию для обратной совместимости
        await Task.CompletedTask;
    }
}
