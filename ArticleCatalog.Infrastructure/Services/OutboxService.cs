using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using System.Text.Json;

namespace ArticleCatalog.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для сохранения доменных событий в Outbox
/// </summary>
public class OutboxService : IOutboxService
{
    private readonly ArticleCatalogDbContext _context;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public OutboxService(ArticleCatalogDbContext context)
    {
        _context = context;
    }

    public async Task SaveEventsToOutboxAsync(IEnumerable<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken = default)
    {
        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var eventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name;
                var eventData = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), JsonOptions);

                var outboxMessage = OutboxMessage.Create(
                    eventType,
                    eventData,
                    domainEvent.OccurredOn);

                outboxMessages.Add(outboxMessage);
            }
        }

        if (outboxMessages.Any())
        {
            await _context.OutboxMessage.AddRangeAsync(outboxMessages, cancellationToken);
        }
    }
}

