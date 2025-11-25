using ArticleCatalog.Domain.Entities;
using ArticleCatalog.Domain.Events;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArticleCatalog.Infrastructure.Services;

/// <summary>
/// Фоновый сервис для обработки сообщений из Outbox
/// </summary>
public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ArticleCatalogDbContext>();

        // Получаем необработанные сообщения (ограничиваем количество для батч-обработки)
        var messages = await dbContext.OutboxMessage
            .Where(m => !m.IsProcessed)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (!messages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                await ProcessMessageAsync(message, scope.ServiceProvider, dbContext, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId} of type {EventType}",
                    message.Id, message.EventType);
                // Продолжаем обработку следующих сообщений
            }
        }
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        IServiceProvider serviceProvider,
        ArticleCatalogDbContext dbContext,
        CancellationToken cancellationToken)
    {
        // Десериализуем событие
        var eventType = Type.GetType(message.EventType);
        if (eventType == null)
        {
            _logger.LogWarning("Event type {EventType} not found for message {MessageId}",
                message.EventType, message.Id);
            message.MarkAsProcessed();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (!typeof(IDomainEvent).IsAssignableFrom(eventType))
        {
            _logger.LogWarning("Type {EventType} is not a domain event for message {MessageId}",
                message.EventType, message.Id);
            message.MarkAsProcessed();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var domainEvent = JsonSerializer.Deserialize(message.EventData, eventType, JsonOptions) as IDomainEvent;
        if (domainEvent == null)
        {
            _logger.LogWarning("Failed to deserialize event for message {MessageId}", message.Id);
            message.MarkAsProcessed();
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        // Публикуем событие через диспетчер
        await DispatchEventAsync(domainEvent, eventType, serviceProvider, cancellationToken);

        // Помечаем сообщение как обработанное
        message.MarkAsProcessed();
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Processed outbox message {MessageId} of type {EventType}",
            message.Id, message.EventType);
    }

    private async Task DispatchEventAsync(
        IDomainEvent domainEvent,
        Type eventType,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        // Используем рефлексию для вызова обработчиков
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod != null)
                {
                    var task = handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken }) as Task;
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling domain event {EventType}",
                    eventType.Name);
                throw;
            }
        }
    }
}

