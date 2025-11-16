namespace ArticleCatalog.Domain.Events;

/// <summary>
/// Доменное событие: статья создана
/// </summary>
public record ArticleCreatedEvent(
    Guid ArticleId,
    string Title,
    IReadOnlyList<string> TagNames,
    DateTimeOffset CreatedAt
) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

