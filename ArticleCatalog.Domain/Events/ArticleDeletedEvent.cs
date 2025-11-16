namespace ArticleCatalog.Domain.Events;

/// <summary>
/// Доменное событие: статья удалена
/// </summary>
public record ArticleDeletedEvent(
    Guid ArticleId,
    IReadOnlyList<string> TagNames
) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

