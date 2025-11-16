namespace ArticleCatalog.Domain.Events;

/// <summary>
/// Доменное событие: теги статьи изменены
/// </summary>
public record ArticleTagsChangedEvent(
    Guid ArticleId,
    IReadOnlyList<string> NewTagNames,
    DateTimeOffset UpdatedAt
) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

