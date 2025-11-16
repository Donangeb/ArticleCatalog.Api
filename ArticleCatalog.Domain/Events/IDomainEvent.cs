namespace ArticleCatalog.Domain.Events;

/// <summary>
/// Маркерный интерфейс для доменных событий
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}

