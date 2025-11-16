using ArticleCatalog.Application.DTOs;
using ArticleCatalog.Application.Interfaces;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Exceptions;
using ArticleCatalog.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.Services;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ITagService _tagService;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(
        IArticleRepository articleRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher,
        ITagService tagService,
        ILogger<ArticleService> logger)
    {
        _articleRepository = articleRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
        _tagService = tagService;
        _logger = logger;
    }

    public async Task<ArticleDto> CreateAsync(CreateArticleRequest request)
    {
        // Получаем или создаем теги
        var tagIds = await _tagService.GetOrCreateManyAsync(request.Tags);
        var tags = await _tagRepository.GetByIdsAsync(tagIds);

        // Создаем статью через фабричный метод агрегата
        var article = Article.Create(request.Title, request.Tags);

        // Устанавливаем теги (это также публикует доменное событие)
        article.SetTags(tagIds, tags, isNewArticle: true);

        // Сохраняем статью
        await _articleRepository.AddAsync(article);
        await _unitOfWork.SaveChangesAsync();

        // Публикуем доменные события
        await _eventDispatcher.DispatchEventsAsync(new[] { article });

        return await BuildDto(article.Id);
    }

    public async Task<ArticleDto> UpdateAsync(Guid id, UpdateArticleRequest request)
    {
        var article = await _articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found");

        // Получаем или создаем теги
        var tagIds = await _tagService.GetOrCreateManyAsync(request.Tags);
        var tags = await _tagRepository.GetByIdsAsync(tagIds);

        // Обновляем через методы агрегата
        article.UpdateTitle(request.Title);
        article.SetTags(tagIds, tags, isNewArticle: false);
        article.MarkAsUpdated();

        // Сохраняем изменения
        await _articleRepository.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync();

        // Публикуем доменные события
        await _eventDispatcher.DispatchEventsAsync(new[] { article });

        return await BuildDto(id);
    }

    public async Task<ArticleDto> GetAsync(Guid id)
    {
        return await BuildDto(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found");

        // Получаем теги для доменного события
        var tagIds = article.ArticleTags.Select(at => at.TagId).ToList();
        var tags = await _tagRepository.GetByIdsAsync(tagIds);
        var tagNames = article.GetTagNames(tags);

        // Публикуем событие удаления перед удалением
        article.MarkAsDeleted(tagNames);

        // Удаляем статью
        await _articleRepository.RemoveAsync(article);
        await _unitOfWork.SaveChangesAsync();

        // Публикуем доменные события
        await _eventDispatcher.DispatchEventsAsync(new[] { article });
    }

    private async Task<ArticleDto> BuildDto(Guid id)
    {
        var article = await _articleRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Article {id} not found");

        // Получаем теги для статьи
        var tagIds = article.ArticleTags.Select(at => at.TagId).ToList();
        var tags = await _tagRepository.GetByIdsAsync(tagIds);
        
        // Используем метод агрегата для получения названий тегов
        var tagNames = article.GetTagNames(tags);

        return new ArticleDto(
            article.Id,
            article.Title,
            article.CreatedAt,
            article.UpdatedAt,
            tagNames
        );
    }
}
