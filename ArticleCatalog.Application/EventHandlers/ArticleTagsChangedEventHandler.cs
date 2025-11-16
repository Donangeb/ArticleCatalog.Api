using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.EventHandlers;

/// <summary>
/// Обработчик события изменения тегов статьи - обновляет разделы
/// </summary>
public class ArticleTagsChangedEventHandler : IDomainEventHandler<ArticleTagsChangedEvent>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArticleTagsChangedEventHandler> _logger;

    public ArticleTagsChangedEventHandler(
        ISectionRepository sectionRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork,
        ILogger<ArticleTagsChangedEventHandler> logger)
    {
        _sectionRepository = sectionRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(ArticleTagsChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagSetKey = TagSetKey.Create(domainEvent.NewTagNames);
            var existingSection = await _sectionRepository.GetByTagSetKeyAsync(tagSetKey, cancellationToken);

            if (existingSection == null)
            {
                // Получаем теги по именам
                var tags = new List<Domain.Entities.Tag>();
                foreach (var tagName in domainEvent.NewTagNames)
                {
                    var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
                    if (tag != null)
                    {
                        tags.Add(tag);
                    }
                }

                if (tags.Count > 0)
                {
                    var section = Domain.Entities.Section.Create(tags);
                    await _sectionRepository.AddAsync(section, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Section created for article {ArticleId} with tags: {Tags}",
                        domainEvent.ArticleId, string.Join(", ", domainEvent.NewTagNames));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ArticleTagsChangedEvent for article {ArticleId}",
                domainEvent.ArticleId);
            throw;
        }
    }
}

