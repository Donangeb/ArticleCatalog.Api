using ArticleCatalog.Domain.Common;
using ArticleCatalog.Domain.Repositories;
using ArticleCatalog.Infrastructure.Data;
using ArticleCatalog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArticleCatalog.Infrastructure.Repositories;

/// <summary>
/// Реализация Unit of Work для управления транзакциями
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ArticleCatalogDbContext _context;
    private readonly IOutboxService _outboxService;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ArticleCatalogDbContext context, IOutboxService outboxService)
    {
        _context = context;
        _outboxService = outboxService;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> SaveChangesWithOutboxAsync(IEnumerable<AggregateRoot<Guid>> aggregates, CancellationToken cancellationToken = default)
    {
        // Сохраняем события в Outbox перед сохранением изменений
        await _outboxService.SaveEventsToOutboxAsync(aggregates, cancellationToken);
        
        // Очищаем события из агрегатов после сохранения в Outbox
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // Сохраняем все изменения (включая Outbox) в одной транзакции
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}

