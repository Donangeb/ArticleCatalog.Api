using ArticleCatalog.Domain.Common;

namespace ArticleCatalog.Domain.Entities;

/// <summary>
/// Сущность для хранения доменных событий в таблице Outbox
/// </summary>
public class OutboxMessage : Entity<Guid>
{
    public string EventType { get; private set; } = string.Empty;
    public string EventData { get; private set; } = string.Empty;
    public DateTimeOffset OccurredOn { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public bool IsProcessed { get; private set; }

    // Для EF Core
    private OutboxMessage() : base() { }

    private OutboxMessage(
        Guid id,
        string eventType,
        string eventData,
        DateTimeOffset occurredOn) : base(id)
    {
        EventType = eventType;
        EventData = eventData;
        OccurredOn = occurredOn;
        CreatedAt = DateTimeOffset.UtcNow;
        IsProcessed = false;
    }

    /// <summary>
    /// Фабричный метод для создания сообщения Outbox
    /// </summary>
    public static OutboxMessage Create(string eventType, string eventData, DateTimeOffset occurredOn)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            eventData,
            occurredOn);
    }

    /// <summary>
    /// Помечает сообщение как обработанное
    /// </summary>
    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTimeOffset.UtcNow;
    }
}

