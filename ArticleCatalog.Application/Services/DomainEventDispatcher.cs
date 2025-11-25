using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArticleCatalog.Application.Services;

/// <summary>
/// Диспетчер доменных событий - публикует события агрегатов
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken = default);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchEventsAsync(IEnumerable<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await DispatchEventAsync(domainEvent, cancellationToken);
            }
        }
    }

    private async Task DispatchEventAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) =>  await DispatcherEventInternalAsync((dynamic)domainEvent, cancellationToken);

    private async Task DispatcherEventInternalAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken) where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching domain event {EventType}", typeof(TEvent).Name);
                throw;
            }
        }
    }
}

