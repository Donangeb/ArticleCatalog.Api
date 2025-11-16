using ArticleCatalog.Domain.Events;

namespace ArticleCatalog.Domain.Common;

/// <summary>
/// Базовый класс для корней агрегатов с поддержкой доменных событий
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Список доменных событий, которые произошли в рамках этого агрегата
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Добавить доменное событие
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Удалить доменное событие
    /// </summary>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Очистить все доменные события (обычно вызывается после сохранения)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

