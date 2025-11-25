using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Events;

namespace ArticleCatalog.Domain.Repositories;

/// <summary>
/// Сервис для сохранения доменных событий в Outbox
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Сохраняет доменные события агрегатов в Outbox
    /// </summary>
    Task SaveEventsToOutboxAsync(IEnumerable<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken = default);
}

