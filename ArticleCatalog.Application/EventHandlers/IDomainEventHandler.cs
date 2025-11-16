using ArticleCatalog.Domain.Events;

namespace ArticleCatalog.Application.EventHandlers;

/// <summary>
/// Интерфейс для обработчиков доменных событий
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

