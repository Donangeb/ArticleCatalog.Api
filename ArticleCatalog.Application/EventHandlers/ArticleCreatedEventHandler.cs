using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.EventHandlers;

/// <summary>
/// Обработчик события создания статьи - автоматически создает раздел для набора тегов
/// </summary>
public class ArticleCreatedEventHandler : IDomainEventHandler<ArticleCreatedEvent>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArticleCreatedEventHandler> _logger;

    public ArticleCreatedEventHandler(
        ISectionRepository sectionRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork,
        ILogger<ArticleCreatedEventHandler> logger)
    {
        _sectionRepository = sectionRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(ArticleCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagSetKey = TagSetKey.Create(domainEvent.TagNames);
            var existingSection = await _sectionRepository.GetByTagSetKeyAsync(tagSetKey, cancellationToken);

            if (existingSection == null)
            {
                // Получаем теги по именам
                var tags = new List<Domain.Entities.Tag>();
                foreach (var tagName in domainEvent.TagNames)
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
                        domainEvent.ArticleId, string.Join(", ", domainEvent.TagNames));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ArticleCreatedEvent for article {ArticleId}",
                domainEvent.ArticleId);
            throw;
        }
    }
}

