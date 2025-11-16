using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.EventHandlers;

/// <summary>
/// Обработчик события удаления статьи - очищает пустые разделы
/// </summary>
public class ArticleDeletedEventHandler : IDomainEventHandler<ArticleDeletedEvent>
{
    private readonly ISectionRepository _sectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArticleDeletedEventHandler> _logger;

    public ArticleDeletedEventHandler(
        ISectionRepository sectionRepository,
        IUnitOfWork unitOfWork,
        ILogger<ArticleDeletedEventHandler> logger)
    {
        _sectionRepository = sectionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(ArticleDeletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var tagSetKey = TagSetKey.Create(domainEvent.TagNames);
            var section = await _sectionRepository.GetByTagSetKeyAsync(tagSetKey, cancellationToken);

            if (section != null)
            {
                var articlesCount = await _sectionRepository.CountArticlesByTagSetKeyAsync(tagSetKey, cancellationToken);

                if (articlesCount == 0)
                {
                    await _sectionRepository.RemoveAsync(section, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Section {SectionId} removed as it has no articles",
                        section.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ArticleDeletedEvent for article {ArticleId}",
                domainEvent.ArticleId);
            throw;
        }
    }
}

